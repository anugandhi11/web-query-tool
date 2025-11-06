-- Initial Database Migration for Web Query Tool
-- Run this script on your PostgreSQL database

-- Create Users table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" VARCHAR(255) PRIMARY KEY,
    "Email" VARCHAR(200) NOT NULL UNIQUE,
    "FullName" VARCHAR(200) NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLoginAt" TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users"("Email");

-- Create DatabaseConnections table
CREATE TABLE IF NOT EXISTS "DatabaseConnections" (
    "Id" VARCHAR(255) PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Type" VARCHAR(50) NOT NULL,
    "Host" VARCHAR(500) NOT NULL,
    "Port" INTEGER NOT NULL,
    "Database" VARCHAR(200) NOT NULL,
    "Username" VARCHAR(200) NOT NULL,
    "EncryptedPassword" TEXT NOT NULL,
    "AdditionalProperties" TEXT NULL,
    "UserId" VARCHAR(255) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastTestedAt" TIMESTAMP NULL,
    "LastTestSuccessful" BOOLEAN NOT NULL DEFAULT FALSE,
    "QueryTimeoutSeconds" INTEGER NOT NULL DEFAULT 60,
    CONSTRAINT "FK_DatabaseConnections_Users" FOREIGN KEY ("UserId")
        REFERENCES "Users"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_DatabaseConnections_UserId" ON "DatabaseConnections"("UserId");
CREATE INDEX IF NOT EXISTS "IX_DatabaseConnections_UserId_Name" ON "DatabaseConnections"("UserId", "Name");

-- Create QueryHistory table
CREATE TABLE IF NOT EXISTS "QueryHistory" (
    "Id" VARCHAR(255) PRIMARY KEY,
    "UserId" VARCHAR(255) NOT NULL,
    "ConnectionId" VARCHAR(255) NOT NULL,
    "SqlQuery" TEXT NOT NULL,
    "SubmissionType" VARCHAR(50) NOT NULL,
    "ExecutedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExecutionTimeMs" BIGINT NOT NULL,
    "RowCount" INTEGER NOT NULL,
    "Success" BOOLEAN NOT NULL,
    "ErrorMessage" TEXT NULL,
    "IpAddress" VARCHAR(50) NULL,
    CONSTRAINT "FK_QueryHistory_Users" FOREIGN KEY ("UserId")
        REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_QueryHistory_DatabaseConnections" FOREIGN KEY ("ConnectionId")
        REFERENCES "DatabaseConnections"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_QueryHistory_UserId" ON "QueryHistory"("UserId");
CREATE INDEX IF NOT EXISTS "IX_QueryHistory_ConnectionId" ON "QueryHistory"("ConnectionId");
CREATE INDEX IF NOT EXISTS "IX_QueryHistory_ExecutedAt" ON "QueryHistory"("ExecutedAt");

-- Insert default admin user
-- Password: Admin@123 (BCrypt hashed)
INSERT INTO "Users" ("Id", "Email", "FullName", "PasswordHash", "Role", "IsActive", "CreatedAt")
VALUES (
    gen_random_uuid()::text,
    'admin@example.com',
    'System Administrator',
    '$2a$11$YQNlqr5z5QEqZGxHQJxAp.8vZ8xGZr5K4QJxD9RqYXH1ZlYq5YqVq',
    'Admin',
    TRUE,
    CURRENT_TIMESTAMP
)
ON CONFLICT ("Email") DO NOTHING;

-- Grant necessary permissions (adjust as needed)
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO postgres;
