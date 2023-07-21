using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrudApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorizationGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuperHero",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SuperHeroName = table.Column<string>(type: "TEXT", nullable: false),
                    CivilName = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperHero", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationGroupEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorizationGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationGroupEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationGroupEntity_AuthorizationGroup_AuthorizationGroupId",
                        column: x => x.AuthorizationGroupId,
                        principalTable: "AuthorizationGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationGroupMembership",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorizationGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorizationRoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationGroupMembership", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationGroupMembership_AuthorizationGroup_AuthorizationGroupId",
                        column: x => x.AuthorizationGroupId,
                        principalTable: "AuthorizationGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorizationGroupMembership_AuthorizationRole_AuthorizationRoleId",
                        column: x => x.AuthorizationRoleId,
                        principalTable: "AuthorizationRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorizationGroupMembership_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityChangeEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangeType = table.Column<string>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", nullable: true),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AuthPrincipalId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Time = table.Column<long>(type: "INTEGER", nullable: false),
                    ActivityId = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityChangeEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityChangeEvent_AuthorizationGroupEntity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "AuthorizationGroupEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityChangeEvent_AuthorizationGroupMembership_EntityId",
                        column: x => x.EntityId,
                        principalTable: "AuthorizationGroupMembership",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityChangeEvent_AuthorizationGroup_EntityId",
                        column: x => x.EntityId,
                        principalTable: "AuthorizationGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityChangeEvent_AuthorizationRole_EntityId",
                        column: x => x.EntityId,
                        principalTable: "AuthorizationRole",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityChangeEvent_EntityChangeEvent_EntityId",
                        column: x => x.EntityId,
                        principalTable: "EntityChangeEvent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityChangeEvent_SuperHero_EntityId",
                        column: x => x.EntityId,
                        principalTable: "SuperHero",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityChangeEvent_User_EntityId",
                        column: x => x.EntityId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyChangeEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityChangeEventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyName = table.Column<string>(type: "TEXT", nullable: false),
                    OldPropertyValue = table.Column<string>(type: "TEXT", nullable: true),
                    NewPropertyValue = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyChangeEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyChangeEvent_EntityChangeEvent_EntityChangeEventId",
                        column: x => x.EntityChangeEventId,
                        principalTable: "EntityChangeEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGroupEntity_AuthorizationGroupId",
                table: "AuthorizationGroupEntity",
                column: "AuthorizationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGroupMembership_AuthorizationGroupId",
                table: "AuthorizationGroupMembership",
                column: "AuthorizationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGroupMembership_AuthorizationRoleId",
                table: "AuthorizationGroupMembership",
                column: "AuthorizationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationGroupMembership_UserId_AuthorizationGroupId_AuthorizationRoleId",
                table: "AuthorizationGroupMembership",
                columns: new[] { "UserId", "AuthorizationGroupId", "AuthorizationRoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityChangeEvent_EntityId",
                table: "EntityChangeEvent",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyChangeEvent_EntityChangeEventId",
                table: "PropertyChangeEvent",
                column: "EntityChangeEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityChangeEvent_PropertyChangeEvent_EntityId",
                table: "EntityChangeEvent",
                column: "EntityId",
                principalTable: "PropertyChangeEvent",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizationGroupEntity_AuthorizationGroup_AuthorizationGroupId",
                table: "AuthorizationGroupEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizationGroupMembership_AuthorizationGroup_AuthorizationGroupId",
                table: "AuthorizationGroupMembership");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityChangeEvent_AuthorizationGroup_EntityId",
                table: "EntityChangeEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizationGroupMembership_AuthorizationRole_AuthorizationRoleId",
                table: "AuthorizationGroupMembership");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityChangeEvent_AuthorizationRole_EntityId",
                table: "EntityChangeEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizationGroupMembership_User_UserId",
                table: "AuthorizationGroupMembership");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityChangeEvent_User_EntityId",
                table: "EntityChangeEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityChangeEvent_AuthorizationGroupEntity_EntityId",
                table: "EntityChangeEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityChangeEvent_AuthorizationGroupMembership_EntityId",
                table: "EntityChangeEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityChangeEvent_PropertyChangeEvent_EntityId",
                table: "EntityChangeEvent");

            migrationBuilder.DropTable(
                name: "AuthorizationGroup");

            migrationBuilder.DropTable(
                name: "AuthorizationRole");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "AuthorizationGroupEntity");

            migrationBuilder.DropTable(
                name: "AuthorizationGroupMembership");

            migrationBuilder.DropTable(
                name: "PropertyChangeEvent");

            migrationBuilder.DropTable(
                name: "EntityChangeEvent");

            migrationBuilder.DropTable(
                name: "SuperHero");
        }
    }
}
