using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
	/// <inheritdoc />
	public partial class AddShipping : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<long>(
				name: "ProductId",
				table: "ProductQuantities",
				type: "bigint",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.CreateTable(
				name: "Shippings",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
					Ward = table.Column<string>(type: "nvarchar(max)", nullable: true),
					District = table.Column<string>(type: "nvarchar(max)", nullable: true),
					City = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Shippings", x => x.Id);
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Shippings");

			migrationBuilder.AlterColumn<int>(
				name: "ProductId",
				table: "ProductQuantities",
				type: "int",
				nullable: false,
				oldClrType: typeof(long),
				oldType: "bigint");
		}
	}
}
