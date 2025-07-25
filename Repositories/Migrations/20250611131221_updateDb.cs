using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class updateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckCourseContent_CourseContent_ContentID",
                table: "CheckCourseContent");

            migrationBuilder.DropForeignKey(
                name: "FK_CheckCourseContent_CourseRegistrations_RegistrationID",
                table: "CheckCourseContent");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseContent_Courses_CourseID",
                table: "CourseContent");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyAnswers_SurveyQuestion_QuestionID",
                table: "SurveyAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestion_Surveys_SurveyID",
                table: "SurveyQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSurveyAnswers_SurveyQuestion_QuestionID",
                table: "UserSurveyAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyQuestion",
                table: "SurveyQuestion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseContent",
                table: "CourseContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CheckCourseContent",
                table: "CheckCourseContent");

            migrationBuilder.RenameTable(
                name: "SurveyQuestion",
                newName: "SurveyQuestions");

            migrationBuilder.RenameTable(
                name: "CourseContent",
                newName: "CourseContents");

            migrationBuilder.RenameTable(
                name: "CheckCourseContent",
                newName: "CheckCourseContents");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyQuestion_SurveyID",
                table: "SurveyQuestions",
                newName: "IX_SurveyQuestions_SurveyID");

            migrationBuilder.RenameIndex(
                name: "IX_CourseContent_CourseID",
                table: "CourseContents",
                newName: "IX_CourseContents_CourseID");

            migrationBuilder.RenameIndex(
                name: "IX_CheckCourseContent_RegistrationID",
                table: "CheckCourseContents",
                newName: "IX_CheckCourseContents_RegistrationID");

            migrationBuilder.RenameIndex(
                name: "IX_CheckCourseContent_ContentID",
                table: "CheckCourseContents",
                newName: "IX_CheckCourseContents_ContentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyQuestions",
                table: "SurveyQuestions",
                column: "QuestionID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseContents",
                table: "CourseContents",
                column: "ContentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheckCourseContents",
                table: "CheckCourseContents",
                column: "CheckID");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentCategoryID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagID);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                columns: table => new
                {
                    NewsArticleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsAticleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Headline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewsContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewsSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    NewsStatus = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedByID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByID = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.NewsArticleID);
                    table.ForeignKey(
                        name: "FK_NewsArticles_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsArticles_Users_CreatedByID",
                        column: x => x.CreatedByID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_NewsArticles_Users_UpdatedByID",
                        column: x => x.UpdatedByID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "NewsTags",
                columns: table => new
                {
                    NewsTagID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsArticleID = table.Column<int>(type: "int", nullable: true),
                    TagID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsTags", x => x.NewsTagID);
                    table.ForeignKey(
                        name: "FK_NewsTags_NewsArticles_NewsArticleID",
                        column: x => x.NewsArticleID,
                        principalTable: "NewsArticles",
                        principalColumn: "NewsArticleID");
                    table.ForeignKey(
                        name: "FK_NewsTags_Tags_TagID",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "TagID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_CategoryID",
                table: "NewsArticles",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_CreatedByID",
                table: "NewsArticles",
                column: "CreatedByID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticles_UpdatedByID",
                table: "NewsArticles",
                column: "UpdatedByID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsTags_NewsArticleID",
                table: "NewsTags",
                column: "NewsArticleID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsTags_TagID",
                table: "NewsTags",
                column: "TagID");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckCourseContents_CourseContents_ContentID",
                table: "CheckCourseContents",
                column: "ContentID",
                principalTable: "CourseContents",
                principalColumn: "ContentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckCourseContents_CourseRegistrations_RegistrationID",
                table: "CheckCourseContents",
                column: "RegistrationID",
                principalTable: "CourseRegistrations",
                principalColumn: "RegistrationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseContents_Courses_CourseID",
                table: "CourseContents",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "CourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyAnswers_SurveyQuestions_QuestionID",
                table: "SurveyAnswers",
                column: "QuestionID",
                principalTable: "SurveyQuestions",
                principalColumn: "QuestionID");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestions_Surveys_SurveyID",
                table: "SurveyQuestions",
                column: "SurveyID",
                principalTable: "Surveys",
                principalColumn: "SurveyID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSurveyAnswers_SurveyQuestions_QuestionID",
                table: "UserSurveyAnswers",
                column: "QuestionID",
                principalTable: "SurveyQuestions",
                principalColumn: "QuestionID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckCourseContents_CourseContents_ContentID",
                table: "CheckCourseContents");

            migrationBuilder.DropForeignKey(
                name: "FK_CheckCourseContents_CourseRegistrations_RegistrationID",
                table: "CheckCourseContents");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseContents_Courses_CourseID",
                table: "CourseContents");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyAnswers_SurveyQuestions_QuestionID",
                table: "SurveyAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestions_Surveys_SurveyID",
                table: "SurveyQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSurveyAnswers_SurveyQuestions_QuestionID",
                table: "UserSurveyAnswers");

            migrationBuilder.DropTable(
                name: "NewsTags");

            migrationBuilder.DropTable(
                name: "NewsArticles");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SurveyQuestions",
                table: "SurveyQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseContents",
                table: "CourseContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CheckCourseContents",
                table: "CheckCourseContents");

            migrationBuilder.RenameTable(
                name: "SurveyQuestions",
                newName: "SurveyQuestion");

            migrationBuilder.RenameTable(
                name: "CourseContents",
                newName: "CourseContent");

            migrationBuilder.RenameTable(
                name: "CheckCourseContents",
                newName: "CheckCourseContent");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyQuestions_SurveyID",
                table: "SurveyQuestion",
                newName: "IX_SurveyQuestion_SurveyID");

            migrationBuilder.RenameIndex(
                name: "IX_CourseContents_CourseID",
                table: "CourseContent",
                newName: "IX_CourseContent_CourseID");

            migrationBuilder.RenameIndex(
                name: "IX_CheckCourseContents_RegistrationID",
                table: "CheckCourseContent",
                newName: "IX_CheckCourseContent_RegistrationID");

            migrationBuilder.RenameIndex(
                name: "IX_CheckCourseContents_ContentID",
                table: "CheckCourseContent",
                newName: "IX_CheckCourseContent_ContentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SurveyQuestion",
                table: "SurveyQuestion",
                column: "QuestionID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseContent",
                table: "CourseContent",
                column: "ContentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheckCourseContent",
                table: "CheckCourseContent",
                column: "CheckID");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckCourseContent_CourseContent_ContentID",
                table: "CheckCourseContent",
                column: "ContentID",
                principalTable: "CourseContent",
                principalColumn: "ContentID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckCourseContent_CourseRegistrations_RegistrationID",
                table: "CheckCourseContent",
                column: "RegistrationID",
                principalTable: "CourseRegistrations",
                principalColumn: "RegistrationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseContent_Courses_CourseID",
                table: "CourseContent",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "CourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyAnswers_SurveyQuestion_QuestionID",
                table: "SurveyAnswers",
                column: "QuestionID",
                principalTable: "SurveyQuestion",
                principalColumn: "QuestionID");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestion_Surveys_SurveyID",
                table: "SurveyQuestion",
                column: "SurveyID",
                principalTable: "Surveys",
                principalColumn: "SurveyID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSurveyAnswers_SurveyQuestion_QuestionID",
                table: "UserSurveyAnswers",
                column: "QuestionID",
                principalTable: "SurveyQuestion",
                principalColumn: "QuestionID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
