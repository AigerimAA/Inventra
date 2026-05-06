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
            migrationBuilder.Sql("CREATE FULLTEXT CATALOG InventraFtsCatalog AS DEFAULT;", suppressTransaction: true);

            migrationBuilder.Sql(@"
                CREATE FULLTEXT INDEX ON Inventories(Title, Description) 
                KEY INDEX PK_Inventories 
                ON InventraFtsCatalog 
                WITH (CHANGE_TRACKING = AUTO);", suppressTransaction: true);

            migrationBuilder.Sql(@"
                CREATE FULLTEXT INDEX ON Items(CustomId, CustomString1Value, CustomString2Value, 
                    CustomString3Value, CustomText1Value, CustomText2Value, CustomText3Value) 
                KEY INDEX PK_Items 
                ON InventraFtsCatalog 
                WITH (CHANGE_TRACKING = AUTO);", suppressTransaction: true);
                }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FULLTEXT INDEX ON Items;", suppressTransaction: true);
            migrationBuilder.Sql("DROP FULLTEXT INDEX ON Inventories;", suppressTransaction: true);
            migrationBuilder.Sql("DROP FULLTEXT CATALOG InventraFtsCatalog;", suppressTransaction: true);
        }
    }
}
