using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowGate.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowEngineFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormDataJson",
                table: "workflow_instances",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutionType",
                table: "approval_steps",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "approval_steps",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StepIndex",
                table: "approval_steps",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormDataJson",
                table: "workflow_instances");

            migrationBuilder.DropColumn(
                name: "ExecutionType",
                table: "approval_steps");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "approval_steps");

            migrationBuilder.DropColumn(
                name: "StepIndex",
                table: "approval_steps");
        }
    }
}
