CREATE TABLE `fso_avatar_activity` (
  `avatar_id` INT(10) UNSIGNED NOT NULL,
  `activity_day` DATE NOT NULL,
  PRIMARY KEY (`avatar_id`, `activity_day`),
  CONSTRAINT `FK_fso_avatar_activity_avatar`
    FOREIGN KEY (`avatar_id`) REFERENCES `fso_avatars` (`avatar_id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) COLLATE='utf8_general_ci'
  ENGINE=InnoDB;

CREATE TABLE `fso_relationship_decay` (
  `from_id` INT(10) UNSIGNED NOT NULL,
  `to_id` INT(10) UNSIGNED NOT NULL,
  `last_contact_day` DATE NOT NULL,
  `active_days_without_contact` INT(10) UNSIGNED NOT NULL DEFAULT 0,
  `last_processed_day` DATE DEFAULT NULL,
  PRIMARY KEY (`from_id`, `to_id`),
  KEY `idx_fso_relationship_decay_to` (`to_id`),
  CONSTRAINT `FK_fso_relationship_decay_from`
    FOREIGN KEY (`from_id`) REFERENCES `fso_avatars` (`avatar_id`)
    ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_fso_relationship_decay_to`
    FOREIGN KEY (`to_id`) REFERENCES `fso_avatars` (`avatar_id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) COLLATE='utf8_general_ci'
  ENGINE=InnoDB;

INSERT INTO `fso_relationship_decay`
  (`from_id`, `to_id`, `last_contact_day`, `active_days_without_contact`, `last_processed_day`)
SELECT DISTINCT rel.from_id, rel.to_id, UTC_DATE(), 0, NULL
FROM `fso_relationships` rel
INNER JOIN `fso_avatars` source_avatar ON source_avatar.avatar_id = rel.from_id
INNER JOIN `fso_avatars` target_avatar ON target_avatar.avatar_id = rel.to_id
WHERE rel.from_id <> rel.to_id;

ALTER TABLE `fso_tasks`
CHANGE COLUMN `task_type` `task_type` ENUM(
  'prune_database',
  'bonus',
  'shutdown',
  'job_balance',
  'multi_check',
  'prune_abandoned_lots',
  'neighborhood_tick',
  'birthday_gift',
  'relationship_decay'
) NOT NULL;
