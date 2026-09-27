using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalCopa26.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Letra = table.Column<char>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Simulacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VisitanteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadaEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Selecoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Tecnico = table.Column<string>(type: "TEXT", nullable: false),
                    CabecaDeChave = table.Column<bool>(type: "INTEGER", nullable: false),
                    GrupoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Selecoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Selecoes_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jogadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Posicao = table.Column<string>(type: "TEXT", nullable: false),
                    Idade = table.Column<int>(type: "INTEGER", nullable: false),
                    Gols = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipacoesCopas = table.Column<int>(type: "INTEGER", nullable: true),
                    SelecaoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogadores_Selecoes_SelecaoId",
                        column: x => x.SelecaoId,
                        principalTable: "Selecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Fase = table.Column<string>(type: "TEXT", nullable: false),
                    Rotulo = table.Column<string>(type: "TEXT", nullable: false),
                    GrupoId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estadio = table.Column<string>(type: "TEXT", nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", nullable: false),
                    MandanteId = table.Column<int>(type: "INTEGER", nullable: true),
                    VisitanteId = table.Column<int>(type: "INTEGER", nullable: true),
                    VagaMandante = table.Column<string>(type: "TEXT", nullable: true),
                    VagaVisitante = table.Column<string>(type: "TEXT", nullable: true),
                    GolsMandante = table.Column<int>(type: "INTEGER", nullable: true),
                    GolsVisitante = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogos_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jogos_Selecoes_MandanteId",
                        column: x => x.MandanteId,
                        principalTable: "Selecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jogos_Selecoes_VisitanteId",
                        column: x => x.VisitanteId,
                        principalTable: "Selecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RankingsFifa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Posicao = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoSelecao = table.Column<string>(type: "TEXT", nullable: false),
                    NomeSelecao = table.Column<string>(type: "TEXT", nullable: false),
                    Pontos = table.Column<double>(type: "REAL", nullable: false),
                    SelecaoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingsFifa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankingsFifa_Selecoes_SelecaoId",
                        column: x => x.SelecaoId,
                        principalTable: "Selecoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SimulacoesJogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SimulacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    JogoId = table.Column<int>(type: "INTEGER", nullable: false),
                    GolsMandante = table.Column<int>(type: "INTEGER", nullable: false),
                    GolsVisitante = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulacoesJogos", x => x.Id);
                    table.CheckConstraint("CK_SimulacaoJogo_GolsMandante_Faixa", "GolsMandante >= 0 AND GolsMandante <= 30");
                    table.CheckConstraint("CK_SimulacaoJogo_GolsVisitante_Faixa", "GolsVisitante >= 0 AND GolsVisitante <= 30");
                    table.ForeignKey(
                        name: "FK_SimulacoesJogos_Jogos_JogoId",
                        column: x => x.JogoId,
                        principalTable: "Jogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SimulacoesJogos_Simulacoes_SimulacaoId",
                        column: x => x.SimulacaoId,
                        principalTable: "Simulacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_Letra",
                table: "Grupos",
                column: "Letra",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_SelecaoId",
                table: "Jogadores",
                column: "SelecaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_GrupoId",
                table: "Jogos",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_MandanteId",
                table: "Jogos",
                column: "MandanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_Numero",
                table: "Jogos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_VisitanteId",
                table: "Jogos",
                column: "VisitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingsFifa_Posicao",
                table: "RankingsFifa",
                column: "Posicao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankingsFifa_SelecaoId",
                table: "RankingsFifa",
                column: "SelecaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Selecoes_Codigo",
                table: "Selecoes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Selecoes_GrupoId",
                table: "Selecoes",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Simulacoes_VisitanteId",
                table: "Simulacoes",
                column: "VisitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulacoesJogos_JogoId",
                table: "SimulacoesJogos",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulacoesJogos_SimulacaoId_JogoId",
                table: "SimulacoesJogos",
                columns: new[] { "SimulacaoId", "JogoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jogadores");

            migrationBuilder.DropTable(
                name: "RankingsFifa");

            migrationBuilder.DropTable(
                name: "SimulacoesJogos");

            migrationBuilder.DropTable(
                name: "Jogos");

            migrationBuilder.DropTable(
                name: "Simulacoes");

            migrationBuilder.DropTable(
                name: "Selecoes");

            migrationBuilder.DropTable(
                name: "Grupos");
        }
    }
}
