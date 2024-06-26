﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIMS.Infrastructure.Persistence.Migrations
{
	public partial class AddImagePublicId : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "ImagePublicId",
				table: "Books",
				type: "nvarchar(max)",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "ImagePublicId",
				table: "Books");
		}
	}
}
