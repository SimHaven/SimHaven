'use strict';

angular.module('admin')
  .controller('EventsCtrl', function ($scope, Api, $mdDialog, $mdToast, $interval) {
      $scope.events = [];
      $scope.presets = [];
      $scope.statusFilter = 'active';

      var toast = function (message) {
          $mdToast.show($mdToast.simple().content(message).position('bottom right').hideDelay(5000));
      };

      var eventType = function (evt) {
          if (evt.type_str) return evt.type_str;
          return ['mail_only', 'free_object', 'free_money', 'free_green', 'obj_tuning'][evt.type] || 'unknown';
      };

      var decorate = function (evt) {
          var now = new Date();
          var start = new Date(evt.start_day);
          var end = new Date(evt.end_day);
          evt.status = start > now ? 'upcoming' : (end < now ? 'expired' : 'active');
          evt.type_name = eventType(evt);
          evt.type_label = {
              obj_tuning: 'Gameplay tuning',
              free_money: 'Simoleon gift',
              free_object: 'Object gift',
              mail_only: 'Mail announcement',
              free_green: 'Green gift'
          }[evt.type_name] || evt.type_name;
          var preset = $scope.presets.filter(function (item) { return item.preset_id === evt.value; })[0];
          evt.preset_name = preset ? preset.name : null;
          return evt;
      };

      var refresh = function () {
          var presetsPromise = Api.all('/events/presets').getList().then(function (presets) {
              $scope.presets = presets;
          });

          return presetsPromise.then(function () {
              $scope.promise = Api.all('/events').getList({ offset: 0, limit: 100, order: 'start_day' }).then(function (events) {
                  $scope.events = events.map(decorate);
              });
              return $scope.promise;
          });
      };

      $scope.visibleEvents = function () {
          return $scope.events.filter(function (evt) { return evt.status === $scope.statusFilter; });
      };

      $scope.count = function (status) {
          return $scope.events.filter(function (evt) { return evt.status === status; }).length;
      };

      var submitEvent = function (result) {
          var createEvent = function (presetId) {
              if (presetId) result.event.value = presetId;
              return Api.all('/events').post(result.event).then(function () {
                  toast('Special event saved. It will be picked up by the city server within one minute.');
                  refresh();
              }, function (error) {
                  var message = error && error.data && (error.data.message || error.data) || error || 'The event could not be saved.';
                  toast(message);
              });
          };

          if (!result.preset) return createEvent();

          return Api.all('/events/presets').post(result.preset).then(function (presets) {
              var matches = presets.filter(function (preset) { return preset.name === result.preset.name; });
              matches.sort(function (a, b) { return b.preset_id - a.preset_id; });
              if (!matches.length) throw new Error('The tuning preset was created, but its ID was not returned.');
              return createEvent(matches[0].preset_id);
          }, function (error) {
              toast(error && error.data || error || 'The tuning preset could not be created.');
          });
      };

      $scope.showAdd = function (event, copy) {
          $mdDialog.show({
              controller: 'EventCreateDialogCtrl',
              templateUrl: 'app/admin/events/event-create.dialog.html',
              parent: angular.element(document.body),
              targetEvent: event,
              clickOutsideToClose: false,
              locals: {
                  presets: $scope.presets,
                  copy: copy || null
              }
          }).then(submitEvent);
      };

      $scope.endNow = function (event, evt) {
          var confirm = $mdDialog.confirm()
              .title('End this event now?')
              .content('The city server will remove its effects from open lots within one minute. The event remains in history.')
              .ariaLabel('End event')
              .ok('End Event')
              .cancel('Keep Running')
              .targetEvent(event);

          $mdDialog.show(confirm).then(function () {
              Api.one('/events', evt.event_id).all('end').post({}).then(function () {
                  toast('Event ended.');
                  refresh();
              }, function () {
                  toast('The event could not be ended. It may already be inactive.');
              });
          });
      };

      refresh();
      var refreshInterval = $interval(refresh, 60000);
      $scope.$on('$destroy', function () { $interval.cancel(refreshInterval); });
  });
