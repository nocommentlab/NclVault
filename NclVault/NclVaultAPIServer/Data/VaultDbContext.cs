using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NclVaultAPIServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Data
{
    public class VaultDbContext : DbContext
    {
        #region Members
        private readonly string STRING_DatabaseName = "vault.db";
        private readonly IConfiguration _configuration;
        private SqliteConnection _sqliteConnection = null;
        #endregion

        #region Database Tables
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<PasswordEntry> Passwords { get; set; }

        #endregion

        public VaultDbContext(DbConnProperties dbConnProperties, IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(dbConnProperties.STRING_Filename)) STRING_DatabaseName = dbConnProperties.STRING_Filename;
            _configuration = configuration;
        }

        public VaultDbContext(SqliteConnection sqliteConnection)
        {
            if (!string.IsNullOrEmpty(sqliteConnection?.DataSource)) STRING_DatabaseName = sqliteConnection.DataSource;

            _sqliteConnection = sqliteConnection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            _sqliteConnection ??= InitializeSQLiteConnection(STRING_DatabaseName, _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "DB_ENCRYPTION_KEY").ToString());
            options.UseSqlite(_sqliteConnection);
        }

        private static SqliteConnection InitializeSQLiteConnection(string databaseFile, string STRING_EncryptionKey)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                Password = STRING_EncryptionKey// PRAGMA key is being sent from EF Core directly after opening the connection
            };
            return new SqliteConnection(connectionString.ToString());
        }

    }
}
