-- Permanent, reusable presets shown by the SimHaven Special Events panel.
-- This migration deliberately reuses presets that already exist by name so a
-- server where these presets were installed manually does not receive copies.

CREATE TEMPORARY TABLE `simhaven_event_preset_definitions` (
    `name` VARCHAR(255) NOT NULL,
    `description` TEXT NOT NULL,
    `tuning_type` VARCHAR(255) NOT NULL,
    `tuning_table` INT NOT NULL,
    `tuning_index` INT NOT NULL,
    `value` FLOAT NOT NULL
);

INSERT INTO `simhaven_event_preset_definitions`
    (`name`, `description`, `tuning_type`, `tuning_table`, `tuning_index`, `value`)
VALUES
    ('FreeSO - April Fools Fire', 'Enables the FreeSO 2017 April Fools fire behavior.', 'special', 0, 0, 1),
    ('FreeSO - April Fools Emoji Only', 'Enables the FreeSO 2018 emoji-only interface behavior.', 'ui', 0, 0, 1),
    ('FreeSO - Pizza Roulette', 'Enables the FreeSO Pizza Roulette event behavior.', 'global.iff', 44, 0, 1),
    ('FreeSO - Hat Towers', 'Enables the FreeSO April Fools hat tower behavior.', 'global.iff', 44, 1, 1),
    ('FreeSO - April Fools Inverted Motives', 'Enables the FreeSO 2020 inverted motives behavior.', 'aprilfools', 0, 2020, 1),
    ('FreeSO - AmongSO', 'Enables the FreeSO AmongSO event behavior.', 'global.iff', 44, 2, 1),
    ('FreeSO - April Fools PeeSO', 'Enables the FreeSO 2024 PeeSO behavior.', 'aprilfools', 0, 2019, 1),
    ('FreeSO - April Fools PeeSO', 'Enables the FreeSO 2024 PeeSO behavior.', 'personglobals.iff', 11, 0, 1),
    ('FreeSO - Summer Heatwave', 'Enables the FreeSO summer heatwave atmosphere.', 'city', 0, 0, 1),
    ('FreeSO - Fructose Monsoon', 'Enables the FreeSO Fructose Monsoon atmosphere.', 'city', 0, 2, 1),
    ('FreeSO - Halloween Candy and Zombies', 'Enables Halloween candy, candy wells, zombies, and related event objects.', 'global.iff', 44, 3, 1),
    ('FreeSO - Halloween Dead World', 'Enables the FreeSO Halloween terrain, atmosphere, and dead-world effects.', 'city', 0, 0, 2),
    ('FreeSO - Halloween Dead World', 'Enables the FreeSO Halloween terrain, atmosphere, and dead-world effects.', 'city', 0, 2, 2),
    ('FreeSO - Halloween Dead World', 'Enables the FreeSO Halloween terrain, atmosphere, and dead-world effects.', 'special', 0, 2, 0.01),
    ('FreeSO - Halloween Complete', 'Enables the Isle of Ghostly Terrors destination, Halloween Candy and Zombies, and the Dead World atmosphere.', 'global.iff', 44, 3, 1),
    ('FreeSO - Halloween Complete', 'Enables the Isle of Ghostly Terrors destination, Halloween Candy and Zombies, and the Dead World atmosphere.', 'city', 0, 0, 2),
    ('FreeSO - Halloween Complete', 'Enables the Isle of Ghostly Terrors destination, Halloween Candy and Zombies, and the Dead World atmosphere.', 'city', 0, 2, 2),
    ('FreeSO - Halloween Complete', 'Enables the Isle of Ghostly Terrors destination, Halloween Candy and Zombies, and the Dead World atmosphere.', 'special', 0, 2, 0.01),
    ('FreeSO - Halloween Complete', 'Enables the Isle of Ghostly Terrors destination, Halloween Candy and Zombies, and the Dead World atmosphere.', 'mailbox.iff', 1, 0, 2),
    ('FreeSO - Halloween Complete', 'Enables the Isle of Ghostly Terrors destination, Halloween Candy and Zombies, and the Dead World atmosphere.', 'leavelot.iff', 7, 0, 2),
    ('FreeSO - Halloween Complete', 'Enables the Isle of Ghostly Terrors destination, Halloween Candy and Zombies, and the Dead World atmosphere.', 'phoneglobals.iff', 0, 0, 522),
    ('FreeSO - Halloween Complete', 'Enables the Isle of Ghostly Terrors destination, Halloween Candy and Zombies, and the Dead World atmosphere.', 'phoneglobals.iff', 0, 1, 1),
    ('FreeSO - Christmas Snowball Fights', 'Enables the FreeSO snowball fight event behavior.', 'global.iff', 44, 4, 1),
    ('FreeSO - Christmas Santa and Tree', 'Enables Santa visits and Christmas tree event behavior.', 'global.iff', 44, 5, 1),
    ('FreeSO - Christmas Snow Weather', 'Enables the FreeSO winter snow weather behavior.', 'global.iff', 44, 6, 1),
    ('FreeSO - Christmas Snow Weather', 'Enables the FreeSO winter snow weather behavior.', 'city', 0, 0, 0),
    ('FreeSO - Christmas Complete', 'Enables snowball fights, Santa and Christmas tree behavior, and snow weather.', 'global.iff', 44, 4, 1),
    ('FreeSO - Christmas Complete', 'Enables snowball fights, Santa and Christmas tree behavior, and snow weather.', 'global.iff', 44, 5, 1),
    ('FreeSO - Christmas Complete', 'Enables snowball fights, Santa and Christmas tree behavior, and snow weather.', 'global.iff', 44, 6, 1),
    ('FreeSO - Christmas Complete', 'Enables snowball fights, Santa and Christmas tree behavior, and snow weather.', 'city', 0, 0, 0);

INSERT INTO `fso_tuning_presets` (`name`, `description`, `flags`)
SELECT definition.`name`, MAX(definition.`description`), 1
FROM `simhaven_event_preset_definitions` definition
LEFT JOIN `fso_tuning_presets` existing
    ON existing.`name` = definition.`name`
WHERE existing.`preset_id` IS NULL
GROUP BY definition.`name`;

-- Keep any existing preset's other flags while marking it reusable.
UPDATE `fso_tuning_presets` preset
INNER JOIN (
    SELECT DISTINCT `name`
    FROM `simhaven_event_preset_definitions`
) definition ON definition.`name` = preset.`name`
SET preset.`flags` = preset.`flags` | 1;

INSERT INTO `fso_tuning_preset_items`
    (`preset_id`, `tuning_type`, `tuning_table`, `tuning_index`, `value`)
SELECT
    preset.`preset_id`,
    definition.`tuning_type`,
    definition.`tuning_table`,
    definition.`tuning_index`,
    definition.`value`
FROM `simhaven_event_preset_definitions` definition
INNER JOIN `fso_tuning_presets` preset
    ON preset.`name` = definition.`name`
   AND preset.`preset_id` = (
       SELECT MIN(named_preset.`preset_id`)
       FROM `fso_tuning_presets` named_preset
       WHERE named_preset.`name` = definition.`name`
   )
WHERE NOT EXISTS (
    SELECT 1
    FROM `fso_tuning_preset_items` existing_item
    WHERE existing_item.`preset_id` = preset.`preset_id`
      AND existing_item.`tuning_type` = definition.`tuning_type`
      AND existing_item.`tuning_table` = definition.`tuning_table`
      AND existing_item.`tuning_index` = definition.`tuning_index`
      AND existing_item.`value` = definition.`value`
);

DROP TEMPORARY TABLE `simhaven_event_preset_definitions`;
