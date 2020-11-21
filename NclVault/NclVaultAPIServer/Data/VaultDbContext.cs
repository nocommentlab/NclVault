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
        private readonly string _STRING_DatabaseName = "vault.db";
        private readonly IConfiguration _configuration;
        private SqliteConnection _sqliteConnection = null;
        #endregion

        #region Database Tables
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<PasswordEntry> Passwords { get; set; }

        #endregion

        public VaultDbContext(IConfiguration configuration)
        {
            _configuration = configuration;

            /* Gets the database filename from the configuration file */
            _STRING_DatabaseName = _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "DB_VAULT_FILENAME").ToString();
            

        }

        public VaultDbContext(SqliteConnection sqliteConnection)
        {
            if (!string.IsNullOrEmpty(sqliteConnection?.DataSource)) _STRING_DatabaseName = sqliteConnection.DataSource;

            _sqliteConnection = sqliteConnection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            
            _sqliteConnection ??= InitializeSQLiteConnection(_STRING_DatabaseName, _configuration.GetSection("NCLVaultConfiguration").GetValue(typeof(string), "DB_ENCRYPTION_KEY").ToString());
            
            options.UseSqlite(_sqliteConnection);
            
        }

        private static SqliteConnection InitializeSQLiteConnection(string databaseFile, string STRING_EncryptionKey)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                Password = STRING_EncryptionKey// Sets the database encryption key
            };

            return new SqliteConnection(connectionString.ToString());
        }

    }
}
