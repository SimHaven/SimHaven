CREATE TABLE IF NOT EXISTS `fso_avatar_top_100` (
  `category` tinyint(3) unsigned NOT NULL,
  `rank` tinyint(3) unsigned NOT NULL,
  `shard_id` int(11) NOT NULL,
  `avatar_id` int(10) unsigned NOT NULL,
  `score` bigint(20) NOT NULL,
  `date` datetime NOT NULL,
  PRIMARY KEY (`shard_id`, `category`, `rank`),
  KEY `idx_fso_avatar_top_100_avatar` (`avatar_id`),
  CONSTRAINT `FK_fso_avatar_top_100_avatar` FOREIGN KEY (`avatar_id`) REFERENCES `fso_avatars` (`avatar_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_fso_avatar_top_100_shard` FOREIGN KEY (`shard_id`) REFERENCES `fso_shards` (`shard_id`) ON DELETE CASCADE ON UPDATE CASCADE
) COLLATE='utf8_general_ci'
  ENGINE=InnoDB;

CREATE PROCEDURE `fso_avatar_top_100_calc_all`(IN `p_shard_id` INT)
    SQL SECURITY INVOKER
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;
        DELETE FROM fso_avatar_top_100 WHERE shard_id = p_shard_id;
        SET @top_timestamp = CURRENT_TIMESTAMP;

        SET @top_rank = 0;
        INSERT INTO fso_avatar_top_100 (category, rank, shard_id, avatar_id, score, date)
            SELECT 0, (@top_rank := @top_rank + 1), p_shard_id, ranked.avatar_id, ranked.score, @top_timestamp
            FROM (
                SELECT ranked_avatar.avatar_id, COUNT(*) AS score
                FROM fso_relationships relationship
                INNER JOIN fso_avatars ranked_avatar
                    ON ranked_avatar.avatar_id = relationship.to_id AND ranked_avatar.shard_id = p_shard_id
                INNER JOIN fso_avatars other_avatar
                    ON other_avatar.avatar_id = relationship.from_id AND other_avatar.shard_id = p_shard_id
                WHERE relationship.`index` = 1 AND relationship.value >= 60
                    AND relationship.from_id <> relationship.to_id
                GROUP BY ranked_avatar.avatar_id
                ORDER BY score DESC, ranked_avatar.avatar_id ASC
                LIMIT 100
            ) ranked;

        SET @top_rank = 0;
        INSERT INTO fso_avatar_top_100 (category, rank, shard_id, avatar_id, score, date)
            SELECT 1, (@top_rank := @top_rank + 1), p_shard_id, ranked.avatar_id, ranked.score, @top_timestamp
            FROM (
                SELECT ranked_avatar.avatar_id, SUM(relationship.value) AS score
                FROM fso_relationships relationship
                INNER JOIN fso_avatars ranked_avatar
                    ON ranked_avatar.avatar_id = relationship.to_id AND ranked_avatar.shard_id = p_shard_id
                INNER JOIN fso_avatars other_avatar
                    ON other_avatar.avatar_id = relationship.from_id AND other_avatar.shard_id = p_shard_id
                WHERE relationship.`index` = 1
                    AND relationship.from_id <> relationship.to_id
                GROUP BY ranked_avatar.avatar_id
                ORDER BY score DESC, ranked_avatar.avatar_id ASC
                LIMIT 100
            ) ranked;

        SET @top_rank = 0;
        INSERT INTO fso_avatar_top_100 (category, rank, shard_id, avatar_id, score, date)
            SELECT 2, (@top_rank := @top_rank + 1), p_shard_id, ranked.avatar_id, ranked.score, @top_timestamp
            FROM (
                SELECT ranked_avatar.avatar_id, COUNT(*) AS score
                FROM fso_relationships relationship
                INNER JOIN fso_avatars ranked_avatar
                    ON ranked_avatar.avatar_id = relationship.from_id AND ranked_avatar.shard_id = p_shard_id
                INNER JOIN fso_avatars other_avatar
                    ON other_avatar.avatar_id = relationship.to_id AND other_avatar.shard_id = p_shard_id
                WHERE relationship.`index` = 1 AND relationship.value >= 60
                    AND relationship.from_id <> relationship.to_id
                GROUP BY ranked_avatar.avatar_id
                ORDER BY score DESC, ranked_avatar.avatar_id ASC
                LIMIT 100
            ) ranked;

        SET @top_rank = 0;
        INSERT INTO fso_avatar_top_100 (category, rank, shard_id, avatar_id, score, date)
            SELECT 3, (@top_rank := @top_rank + 1), p_shard_id, ranked.avatar_id, ranked.score, @top_timestamp
            FROM (
                SELECT ranked_avatar.avatar_id, COUNT(*) AS score
                FROM fso_relationships relationship
                INNER JOIN fso_avatars ranked_avatar
                    ON ranked_avatar.avatar_id = relationship.to_id AND ranked_avatar.shard_id = p_shard_id
                INNER JOIN fso_avatars other_avatar
                    ON other_avatar.avatar_id = relationship.from_id AND other_avatar.shard_id = p_shard_id
                WHERE relationship.`index` = 1 AND relationship.value <= -60
                    AND relationship.from_id <> relationship.to_id
                GROUP BY ranked_avatar.avatar_id
                ORDER BY score DESC, ranked_avatar.avatar_id ASC
                LIMIT 100
            ) ranked;

        SET @top_rank = 0;
        INSERT INTO fso_avatar_top_100 (category, rank, shard_id, avatar_id, score, date)
            SELECT 4, (@top_rank := @top_rank + 1), p_shard_id, ranked.avatar_id, ranked.score, @top_timestamp
            FROM (
                SELECT ranked_avatar.avatar_id, COUNT(*) AS score
                FROM fso_relationships relationship
                INNER JOIN fso_avatars ranked_avatar
                    ON ranked_avatar.avatar_id = relationship.from_id AND ranked_avatar.shard_id = p_shard_id
                INNER JOIN fso_avatars other_avatar
                    ON other_avatar.avatar_id = relationship.to_id AND other_avatar.shard_id = p_shard_id
                WHERE relationship.`index` = 1 AND relationship.value <= -60
                    AND relationship.from_id <> relationship.to_id
                GROUP BY ranked_avatar.avatar_id
                ORDER BY score DESC, ranked_avatar.avatar_id ASC
                LIMIT 100
            ) ranked;
    COMMIT;
END;
