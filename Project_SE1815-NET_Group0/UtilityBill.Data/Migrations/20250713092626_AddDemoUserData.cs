using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UtilityBill.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoUserData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert demo users with BCrypt hashed passwords
            migrationBuilder.Sql(@"INSERT INTO [Users] (Id, UserName, Email, PasswordHash, FullName, IsActive, CreatedAt)
            VALUES
                ('00000000-0000-0000-0000-000000000001', 'admin', 'admin@example.com', '$2a$11$ylQo4YJw7X9g3IRgKH77TOvL7xNGm7uy/F92.tQDHVqjEDWZ9eGJG', 'Administrator', 1, GETDATE()),
                ('00000000-0000-0000-0000-000000000002', 'nguyenvana', 'nguyenvana@example.com', '$2a$11$hX.z4ekykcuJD0T0Gu0xrOJ9oLoFaSHUUh797IrxNCTKmRXcHBzPO', N'Nguyễn Văn A', 1, GETDATE()),
                ('00000000-0000-0000-0000-000000000003', 'tranvanb', 'tranvanb@example.com', '$2a$11$hX.z4ekykcuJD0T0Gu0xrOJ9oLoFaSHUUh797IrxNCTKmRXcHBzPO', N'Trần Văn B', 1, GETDATE()),
                ('00000000-0000-0000-0000-000000000004', 'lethic', 'lethic@example.com', '$2a$11$hX.z4ekykcuJD0T0Gu0xrOJ9oLoFaSHUUh797IrxNCTKmRXcHBzPO', N'Lê Thị C', 1, GETDATE())
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
