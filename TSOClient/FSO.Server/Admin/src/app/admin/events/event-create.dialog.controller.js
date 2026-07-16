'use strict';

angular.module('admin')
  .controller('EventCreateDialogCtrl', function ($scope, $mdDialog, presets, copy) {
      $scope.copy = copy;
      var now = new Date();
      var tomorrow = new Date(now.getTime() + (24 * 60 * 60 * 1000));

      $scope.presets = presets;
      $scope.req = {
          kind: 'boost',
          title: '',
          description: '',
          start_local: now,
          end_local: tomorrow,
          skill_boost: 0,
          money_boost: 0,
          season: 'none',
          existing_preset_id: null,
          amount: 10000,
          send_mail: false,
          mail_subject: '',
          mail_message: '',
          mail_sender_name: 'SimHaven'
      };

      var typeName = function (evt) {
          if (evt.type_str) return evt.type_str;
          return ['mail_only', 'free_object', 'free_money', 'free_green', 'obj_tuning'][evt.type];
      };

      if (copy) {
          var copiedType = typeName(copy);
          $scope.req.title = (copy.title || 'Special Event') + ' (Copy)';
          $scope.req.description = copy.description || '';
          $scope.req.start_local = now;
          $scope.req.end_local = tomorrow;
          $scope.req.send_mail = !!copy.mail_message;
          $scope.req.mail_subject = copy.mail_subject || '';
          $scope.req.mail_message = copy.mail_message || '';
          $scope.req.mail_sender_name = copy.mail_sender_name || 'SimHaven';

          if (copiedType === 'obj_tuning') {
              $scope.req.kind = 'preset';
              $scope.req.existing_preset_id = copy.value;
          } else if (copiedType === 'free_money') {
              $scope.req.kind = 'money';
              $scope.req.amount = copy.value;
          } else {
              $scope.req.kind = 'mail';
          }
      }

      $scope.startNow = function () {
          $scope.req.start_local = new Date();
      };

      $scope.hasTuning = function () {
          return $scope.req.skill_boost > 0 || $scope.req.money_boost > 0 || $scope.req.season !== 'none';
      };

      $scope.cancel = function () { $mdDialog.cancel(); };

      $scope.ok = function () {
          var req = $scope.req;
          var result = {
              event: {
                  title: req.title,
                  description: req.description,
                  start_day: req.start_local.toISOString(),
                  end_day: req.end_local.toISOString(),
                  type: 'mail_only',
                  value: 0,
                  value2: 0,
                  mail_subject: req.send_mail || req.kind === 'mail' ? (req.mail_subject || req.title) : null,
                  mail_message: req.send_mail || req.kind === 'mail' ? req.mail_message : null,
                  mail_sender: -2147483648,
                  mail_sender_name: req.mail_sender_name || 'SimHaven'
              }
          };

          if (req.kind === 'money') {
              result.event.type = 'free_money';
              result.event.value = req.amount;
          } else if (req.kind === 'preset') {
              result.event.type = 'obj_tuning';
              result.event.value = req.existing_preset_id;
          } else if (req.kind === 'boost') {
              result.event.type = 'obj_tuning';
              result.preset = {
                  name: 'SimHaven Event - ' + req.title + ' - ' + Date.now(),
                  description: req.description || 'Created by the SimHaven Special Events panel.',
                  flags: 0,
                  items: []
              };

              if (req.skill_boost > 0) {
                  result.preset.items.push({
                      tuning_type: 'skillobjects.iff',
                      tuning_table: 0,
                      tuning_index: 0,
                      value: Math.round(10 * (1 + req.skill_boost / 100))
                  });
              }
              if (req.money_boost > 0) {
                  result.preset.items.push({
                      tuning_type: 'skillobjects.iff',
                      tuning_table: 4,
                      tuning_index: 6,
                      value: 100 + req.money_boost
                  });
              }
              if (req.season !== 'none') {
                  result.preset.items.push({
                      tuning_type: 'city',
                      tuning_table: 0,
                      tuning_index: 0,
                      value: { winter: 0, summer: 1, autumn: 2 }[req.season]
                  });
              }
          }

          $mdDialog.hide(result);
      };
  });
