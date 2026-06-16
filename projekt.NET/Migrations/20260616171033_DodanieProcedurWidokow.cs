using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projekt.NET.Migrations
{
    /// <inheritdoc />
    public partial class DodanieProcedurWidokow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW vw_GameStatistics AS
                SELECT 
                    g.Id AS GameId, 
                    g.Title, 
                    COUNT(DISTINCT ug.UserId) AS OwnersCount, 
                    AVG(r.Rating) AS AverageRating, 
                    COUNT(DISTINCT r.Id) AS ReviewCount
                FROM Games g
                LEFT JOIN UserGames ug ON g.Id = ug.GameId
                LEFT JOIN Reviews r ON g.Id = r.GameId
                GROUP BY g.Id, g.Title;
            ");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW vw_PublicProfiles AS
                SELECT 
                    Id, 
                    DisplayName, 
                    AvatarPath, 
                    CreatedAt, 
                    ProfilePictureUrl,
                    FirstName,
                    LastName
                FROM AspNetUsers;
            ");

            // --- PROCEDURY SKŁADOWANE (PROCEDURES) ---
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS sp_UpdateGameAverageRating;
                CREATE PROCEDURE sp_UpdateGameAverageRating (IN p_GameId INT)
                BEGIN
                    DECLARE v_AvgRating DOUBLE;

                    SELECT IFNULL(AVG(Rating), 0) INTO v_AvgRating 
                    FROM Reviews 
                    WHERE GameId = p_GameId;

                    UPDATE Games 
                    SET AverageRating = v_AvgRating 
                    WHERE Id = p_GameId;
                END;
            ");

            // --- FUNKCJE (FUNCTIONS) ---
            migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS fn_GetUserActivityBadge;
                CREATE FUNCTION fn_GetUserActivityBadge (p_UserId VARCHAR(255)) 
                RETURNS VARCHAR(50)
                READS SQL DATA
                BEGIN
                    DECLARE v_ReviewCount INT;
                    DECLARE v_PostCount INT;
                    DECLARE v_TotalActivity INT;
                    DECLARE v_Badge VARCHAR(50);

                    SELECT COUNT(*) INTO v_ReviewCount FROM Reviews WHERE UserId = p_UserId;
                    SELECT COUNT(*) INTO v_PostCount FROM ForumPosts WHERE UserId = p_UserId;
                    
                    SET v_TotalActivity = v_ReviewCount + v_PostCount;

                    IF v_TotalActivity > 50 THEN
                        SET v_Badge = 'Weteran';
                    ELSEIF v_TotalActivity > 10 THEN
                        SET v_Badge = 'Aktywny Gracz';
                    ELSE
                        SET v_Badge = 'Początkujący';
                    END IF;

                    RETURN v_Badge;
                END;
            ");

            // --- INDEKSY I OPTYMALIZACJA ---
            // Indeksy B-Tree z dokumentu pdf
            migrationBuilder.Sql("CREATE INDEX idx_games_releasedate ON Games(ReleaseDate);");
            migrationBuilder.Sql("CREATE INDEX idx_games_producer_release ON Games (ProducerId, ReleaseDate);");

            // Indeks do optymalizacji Title z poprzednich rozmów (likwidacja Full Table Scan)
            migrationBuilder.Sql("CREATE INDEX idx_games_title ON Games(Title(255));");

            // Indeks Full-Text Search dla forum
            migrationBuilder.Sql("ALTER TABLE ForumPosts ADD FULLTEXT INDEX ft_idx_forum_content (Title, Content);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE ForumPosts DROP INDEX ft_idx_forum_content;");
            migrationBuilder.Sql("ALTER TABLE Games DROP INDEX idx_games_title;");
            migrationBuilder.Sql("ALTER TABLE Games DROP INDEX idx_games_producer_release;");
            migrationBuilder.Sql("ALTER TABLE Games DROP INDEX idx_games_releasedate;");

            // Usuwanie funkcji i procedur
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_GetUserActivityBadge;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_UpdateGameAverageRating;");

            // Usuwanie widoków
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_PublicProfiles;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_GameStatistics;");
        }
    }
}
