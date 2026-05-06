using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1
            BEGIN
                IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'InventraFtsCatalog')
                BEGIN
                    CREATE FULLTEXT CATALOG InventraFtsCatalog AS DEFAULT;
                END

                IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Inventories'))
                BEGIN
                    CREATE FULLTEXT INDEX ON Inventories(Title, Description) 
                    KEY INDEX PK_Inventories 
                    ON InventraFtsCatalog 
                    WITH (CHANGE_TRACKING = AUTO);
                END

                IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Items'))
                BEGIN
                    CREATE FULLTEXT INDEX ON Items(
                        CustomId, CustomString1Value, CustomString2Value, 
                        CustomString3Value, CustomText1Value, CustomText2Value, CustomText3Value
                    ) 
                    KEY INDEX PK_Items 
                    ON InventraFtsCatalog 
                    WITH (CHANGE_TRACKING = AUTO);
                END
            END
            ", suppressTransaction: true);
                    }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 1
                BEGIN
                    IF EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Items'))
                        DROP FULLTEXT INDEX ON Items;

                    IF EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Inventories'))
                        DROP FULLTEXT INDEX ON Inventories;

                    IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'InventraFtsCatalog')
                        DROP FULLTEXT CATALOG InventraFtsCatalog;
                END
                ", suppressTransaction: true);
        }
    }
}
