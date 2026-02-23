using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralKitchen_Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCentralKitchenDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    item_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__items__3213E83FD184E8CC", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    location_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__location__3213E83F14C50CBF", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__roles__3213E83F5CBCC5E7", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_parameters",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    param_key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    param_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__system_p__3213E83FFD6792C9", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quality_feedbacks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    feedback_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quality___3213E83FA6343B80", x => x.id);
                    table.ForeignKey(
                        name: "fk_feedback_item",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    finished_item_id = table.Column<int>(type: "int", nullable: false),
                    ingredient_item_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__recipes__3213E83FA1ACC16F", x => x.id);
                    table.ForeignKey(
                        name: "fk_recipe_finished_item",
                        column: x => x.finished_item_id,
                        principalTable: "items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_recipe_ingredient_item",
                        column: x => x.ingredient_item_id,
                        principalTable: "items",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inventory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    location_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__inventor__3213E83F66D05270", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventory_item",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_inventory_location",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "productions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    location_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    production_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__producti__3213E83F5D2D53C0", x => x.id);
                    table.ForeignKey(
                        name: "fk_production_item",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_production_location",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "shipments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    from_location_id = table.Column<int>(type: "int", nullable: false),
                    shipment_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__shipment__3213E83FA227DCFA", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipment_location",
                        column: x => x.from_location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stock_checks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    location_id = table.Column<int>(type: "int", nullable: false),
                    check_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__stock_ch__3213E83FE3C0E1F5", x => x.id);
                    table.ForeignKey(
                        name: "fk_stockcheck_location",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__3213E83F04B34198", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_roles",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inventory_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    inventory_id = table.Column<int>(type: "int", nullable: false),
                    tx_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    reference_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__inventor__3213E83F77EEC3AC", x => x.id);
                    table.ForeignKey(
                        name: "fk_inv_tx_inventory",
                        column: x => x.inventory_id,
                        principalTable: "inventory",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "shipment_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    shipment_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__shipment__3213E83FEE1E3CB0", x => x.id);
                    table.ForeignKey(
                        name: "fk_shipmentline_item",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_shipmentline_shipment",
                        column: x => x.shipment_id,
                        principalTable: "shipments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    order_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__orders__3213E83FAAD7D77A", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "material_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    requested_by_user_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_material_request_order",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_material_request_user",
                        column: x => x.requested_by_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__order_li__3213E83FB535DE9E", x => x.id);
                    table.ForeignKey(
                        name: "fk_orderline_item",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orderline_order",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "material_request_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    material_request_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    requested_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    current_stock = table.Column<decimal>(type: "decimal(18,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_request_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_mrl_item",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_mrl_material_request",
                        column: x => x.material_request_id,
                        principalTable: "material_requests",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_item_id",
                table: "inventory",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_location_id",
                table: "inventory",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_transactions_inventory_id",
                table: "inventory_transactions",
                column: "inventory_id");

            migrationBuilder.CreateIndex(
                name: "IX_material_request_lines_item_id",
                table: "material_request_lines",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_material_request_lines_material_request_id",
                table: "material_request_lines",
                column: "material_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_material_requests_order_id",
                table: "material_requests",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_material_requests_requested_by_user_id",
                table: "material_requests",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_lines_item_id",
                table: "order_lines",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_lines_order_id",
                table: "order_lines",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_productions_item_id",
                table: "productions",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_productions_location_id",
                table: "productions",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_feedbacks_item_id",
                table: "quality_feedbacks",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_recipes_finished_item_id",
                table: "recipes",
                column: "finished_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_recipes_ingredient_item_id",
                table: "recipes",
                column: "ingredient_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lines_item_id",
                table: "shipment_lines",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_lines_shipment_id",
                table: "shipment_lines",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipments_from_location_id",
                table: "shipments",
                column: "from_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_checks_location_id",
                table: "stock_checks",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "UQ__system_p__18BAEC9F2EAA2875",
                table: "system_parameters",
                column: "param_key",
                unique: true,
                filter: "[param_key] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_transactions");

            migrationBuilder.DropTable(
                name: "material_request_lines");

            migrationBuilder.DropTable(
                name: "order_lines");

            migrationBuilder.DropTable(
                name: "productions");

            migrationBuilder.DropTable(
                name: "quality_feedbacks");

            migrationBuilder.DropTable(
                name: "recipes");

            migrationBuilder.DropTable(
                name: "shipment_lines");

            migrationBuilder.DropTable(
                name: "stock_checks");

            migrationBuilder.DropTable(
                name: "system_parameters");

            migrationBuilder.DropTable(
                name: "inventory");

            migrationBuilder.DropTable(
                name: "material_requests");

            migrationBuilder.DropTable(
                name: "shipments");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
