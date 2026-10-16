using System;
using System.Data.Common;
using System.Threading.Tasks;
using Api.Security;

namespace Api.Main
{
    public static class DatabaseInitializer
    {
        private const string InitializeSchemaSql = @"
PRAGMA foreign_keys = ON;

-- =========================
-- USERS
-- =========================
CREATE TABLE IF NOT EXISTS users (
    user_id INTEGER PRIMARY KEY AUTOINCREMENT,
    full_name TEXT NOT NULL,
    username TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    role TEXT NOT NULL CHECK(role IN ('Admin','Technician','Staff')),
    phone TEXT,
    email TEXT,
    status TEXT DEFAULT 'Active' CHECK(status IN ('Active','Inactive')),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- =========================
-- CUSTOMERS
-- =========================
CREATE TABLE IF NOT EXISTS customers (
    customer_id INTEGER PRIMARY KEY AUTOINCREMENT,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    phone TEXT,
    email TEXT,
    address TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- =========================
-- AIRCON UNITS
-- =========================
CREATE TABLE IF NOT EXISTS aircon_units (
    unit_id INTEGER PRIMARY KEY AUTOINCREMENT,
    customer_id INTEGER NOT NULL,
    brand TEXT,
    model TEXT,
    horsepower REAL,
    serial_number TEXT UNIQUE,
    installation_date DATE,
    warranty_expiry DATE,
    location TEXT,
    status TEXT DEFAULT 'Active' CHECK(status IN ('Active','Inactive','Under Repair')),
    FOREIGN KEY (customer_id) REFERENCES customers(customer_id) ON DELETE CASCADE
);

-- =========================
-- MAINTENANCE SCHEDULE
-- =========================
CREATE TABLE IF NOT EXISTS maintenance_schedule (
    schedule_id INTEGER PRIMARY KEY AUTOINCREMENT,
    customer_id INTEGER NOT NULL,
    unit_id INTEGER NOT NULL,
    technician_id INTEGER NOT NULL,
    schedule_date DATE NOT NULL,
    schedule_time TIME NOT NULL,
    status TEXT DEFAULT 'Scheduled' CHECK(status IN ('Scheduled','Completed','Cancelled')),
    remarks TEXT,
    FOREIGN KEY (customer_id) REFERENCES customers(customer_id),
    FOREIGN KEY (unit_id) REFERENCES aircon_units(unit_id),
    FOREIGN KEY (technician_id) REFERENCES users(user_id)
);

-- =========================
-- MAINTENANCE RECORDS
-- =========================
CREATE TABLE IF NOT EXISTS maintenance_records (
    record_id INTEGER PRIMARY KEY AUTOINCREMENT,
    schedule_id INTEGER NOT NULL UNIQUE,
    work_performed TEXT,
    parts_replaced TEXT,
    refrigerant_added TEXT,
    technician_notes TEXT,
    next_service_date DATE,
    completed_date DATE,
    FOREIGN KEY (schedule_id) REFERENCES maintenance_schedule(schedule_id) ON DELETE CASCADE
);

-- =========================
-- SPARE PARTS
-- =========================
CREATE TABLE IF NOT EXISTS spare_parts (
    part_id INTEGER PRIMARY KEY AUTOINCREMENT,
    part_name TEXT NOT NULL,
    category TEXT,
    quantity INTEGER DEFAULT 0,
    unit_price REAL,
    stock_status TEXT DEFAULT 'In Stock' CHECK(stock_status IN ('In Stock','Low Stock','Out of Stock'))
);

-- =========================
-- PARTS USAGE
-- =========================
CREATE TABLE IF NOT EXISTS parts_usage (
    usage_id INTEGER PRIMARY KEY AUTOINCREMENT,
    record_id INTEGER NOT NULL,
    part_id INTEGER NOT NULL,
    quantity_used INTEGER NOT NULL,
    FOREIGN KEY (record_id) REFERENCES maintenance_records(record_id) ON DELETE CASCADE,
    FOREIGN KEY (part_id) REFERENCES spare_parts(part_id)
);

-- =========================
-- SERVICE REQUESTS
-- =========================
CREATE TABLE IF NOT EXISTS service_requests (
    request_id INTEGER PRIMARY KEY AUTOINCREMENT,
    customer_id INTEGER NOT NULL,
    unit_id INTEGER NOT NULL,
    issue_description TEXT NOT NULL,
    priority TEXT DEFAULT 'Medium' CHECK(priority IN ('High','Medium','Low')),
    status TEXT DEFAULT 'Open' CHECK(status IN ('Open','Assigned','In Progress','Completed')),
    assigned_technician INTEGER,
    request_date DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (customer_id) REFERENCES customers(customer_id),
    FOREIGN KEY (unit_id) REFERENCES aircon_units(unit_id),
    FOREIGN KEY (assigned_technician) REFERENCES users(user_id)
);

-- =========================
-- NOTIFICATIONS
-- =========================
CREATE TABLE IF NOT EXISTS notifications (
    notification_id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    title TEXT,
    message TEXT,
    is_read INTEGER DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);

-- =========================
-- STORES / BRANCHES
-- =========================
CREATE TABLE IF NOT EXISTS stores (
    store_id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    code TEXT NOT NULL UNIQUE,
    status TEXT DEFAULT 'Open' CHECK(status IN ('Open','Temporarily closed','Closed permanently')),
    is_main_branch INTEGER DEFAULT 0,
    street TEXT,
    barangay TEXT,
    city_municipality TEXT,
    province TEXT,
    region TEXT,
    postal_code TEXT,
    latitude REAL NOT NULL,
    longitude REAL NOT NULL,
    landmark_notes TEXT,
    location_source TEXT DEFAULT 'Pin dropped',
    is_verified INTEGER DEFAULT 0,
    phone TEXT,
    mobile TEXT,
    email TEXT,
    website_url TEXT,
    opening_hours TEXT,
    service_radius_km REAL DEFAULT 15.0,
    services_offered TEXT,
    accepts_walk_ins INTEGER DEFAULT 1,
    parking_notes TEXT,
    manager_id INTEGER,
    assigned_technicians TEXT,
    business_name TEXT,
    registration_number TEXT,
    logo_url TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (manager_id) REFERENCES users(user_id)
);
";

        public static async Task InitializeDatabaseAsync(MyCon db)
        {
            await using var conn = db.GetConnection();
            await conn.OpenAsync();

            // 1. Create tables if missing
            await using (var cmd = db.CreateCommand(conn, InitializeSchemaSql))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // 2. Dynamic column migration for existing database files
            await EnsureColumnExistsAsync(conn, db, "aircon_units", "horsepower", "REAL");
            await EnsureColumnExistsAsync(conn, db, "aircon_units", "serial_number", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "aircon_units", "installation_date", "DATE");
            await EnsureColumnExistsAsync(conn, db, "aircon_units", "warranty_expiry", "DATE");
            await EnsureColumnExistsAsync(conn, db, "aircon_units", "location", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "aircon_units", "status", "TEXT DEFAULT 'Active'");

            await EnsureColumnExistsAsync(conn, db, "maintenance_schedule", "schedule_date", "DATE");
            await EnsureColumnExistsAsync(conn, db, "maintenance_schedule", "schedule_time", "TIME");
            await EnsureColumnExistsAsync(conn, db, "maintenance_schedule", "status", "TEXT DEFAULT 'Scheduled'");
            await EnsureColumnExistsAsync(conn, db, "maintenance_schedule", "remarks", "TEXT");

            await EnsureColumnExistsAsync(conn, db, "maintenance_records", "work_performed", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "maintenance_records", "parts_replaced", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "maintenance_records", "refrigerant_added", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "maintenance_records", "technician_notes", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "maintenance_records", "next_service_date", "DATE");
            await EnsureColumnExistsAsync(conn, db, "maintenance_records", "completed_date", "DATE");

            await EnsureColumnExistsAsync(conn, db, "spare_parts", "category", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "spare_parts", "quantity", "INTEGER DEFAULT 0");
            await EnsureColumnExistsAsync(conn, db, "spare_parts", "unit_price", "REAL");
            await EnsureColumnExistsAsync(conn, db, "spare_parts", "stock_status", "TEXT DEFAULT 'In Stock'");

            await EnsureColumnExistsAsync(conn, db, "service_requests", "priority", "TEXT DEFAULT 'Medium'");
            await EnsureColumnExistsAsync(conn, db, "service_requests", "assigned_technician", "INTEGER");
            await EnsureColumnExistsAsync(conn, db, "service_requests", "request_date", "DATE DEFAULT CURRENT_DATE");
            await EnsureColumnExistsAsync(conn, db, "service_requests", "store_id", "INTEGER REFERENCES stores(store_id)");

            await EnsureColumnExistsAsync(conn, db, "notifications", "title", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "notifications", "message", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "notifications", "is_read", "INTEGER DEFAULT 0");

            // User store association
            await EnsureColumnExistsAsync(conn, db, "users", "store_id", "INTEGER REFERENCES stores(store_id)");

            // Customer address components and geographic coordinates & store association
            await EnsureColumnExistsAsync(conn, db, "customers", "latitude", "REAL");
            await EnsureColumnExistsAsync(conn, db, "customers", "longitude", "REAL");
            await EnsureColumnExistsAsync(conn, db, "customers", "region_code", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "region_name", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "province_code", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "province_name", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "city_code", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "city_name", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "barangay_code", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "barangay_name", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "street_address", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "customers", "store_id", "INTEGER REFERENCES stores(store_id)");

            // Stores table safety columns
            await EnsureColumnExistsAsync(conn, db, "stores", "is_main_branch", "INTEGER DEFAULT 0");
            await EnsureColumnExistsAsync(conn, db, "stores", "service_radius_km", "REAL DEFAULT 15.0");
            await EnsureColumnExistsAsync(conn, db, "stores", "opening_hours", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "stores", "services_offered", "TEXT");
            await EnsureColumnExistsAsync(conn, db, "stores", "accepts_walk_ins", "INTEGER DEFAULT 1");
            await EnsureColumnExistsAsync(conn, db, "stores", "manager_id", "INTEGER");
            await EnsureColumnExistsAsync(conn, db, "stores", "assigned_technicians", "TEXT");

            // 3. Schema migrations: fix outdated CHECK constraints and column names via table recreation
            await MigrateServiceRequestsTableAsync(conn, db);
            await MigrateMaintenanceScheduleTableAsync(conn, db);
            await MigrateMaintenanceRecordsTableAsync(conn, db);

            // 4. Seed Default Admin User if not exists
            const string checkAdminSql = "SELECT COUNT(*) FROM users WHERE username = 'administrator'";
            await using var checkCmd = db.CreateCommand(conn, checkAdminSql);
            var countObj = await checkCmd.ExecuteScalarAsync();
            long count = countObj is long l ? l : Convert.ToInt64(countObj ?? 0);

            if (count == 0)
            {
                string hashedPassword = PasswordHasher.Hash("admin123");
                const string insertAdminSql = @"
                    INSERT INTO users (full_name, username, password, role, status)
                    VALUES ('System Administrator', 'administrator', @password, 'Admin', 'Active')";

                await using var insertCmd = db.CreateCommand(conn, insertAdminSql);
                var p = insertCmd.CreateParameter();
                p.ParameterName = "@password";
                p.Value = hashedPassword;
                insertCmd.Parameters.Add(p);

                await insertCmd.ExecuteNonQueryAsync();
            }

            // 5. Seed Default Store / Main Branch if none exists
            const string checkStoreSql = "SELECT COUNT(*) FROM stores";
            await using var checkStoreCmd = db.CreateCommand(conn, checkStoreSql);
            var storeCountObj = await checkStoreCmd.ExecuteScalarAsync();
            long storeCount = storeCountObj is long sc ? sc : Convert.ToInt64(storeCountObj ?? 0);

            if (storeCount == 0)
            {
                const string insertStoreSql = @"
                    INSERT INTO stores (
                        name, code, status, is_main_branch,
                        street, barangay, city_municipality, province, region, postal_code,
                        latitude, longitude, landmark_notes, location_source, is_verified,
                        phone, mobile, email, website_url,
                        opening_hours, service_radius_km, services_offered, accepts_walk_ins, parking_notes,
                        business_name, registration_number
                    ) VALUES (
                        'AeroFlow Olongapo Main Branch (Sample)', 'OLG-01', 'Open', 1,
                        'Rizal Avenue Extension, West Bajac-Bajac', 'West Bajac-Bajac', 'Olongapo City', 'Zambales', 'Central Luzon (Region III)', '2200',
                        14.8344, 120.2828, 'Across from Gordon College, near Victory Liner Terminal', 'Pin dropped', 1,
                        '(047) 222-3849', '0917-555-8921', 'olongapo.branch@aeroflow.ph', 'https://aeroflow.ph',
                        '{""monday"":{""open"":""08:00"",""close"":""17:00"",""closed"":false},""tuesday"":{""open"":""08:00"",""close"":""17:00"",""closed"":false},""wednesday"":{""open"":""08:00"",""close"":""17:00"",""closed"":false},""thursday"":{""open"":""08:00"",""close"":""17:00"",""closed"":false},""friday"":{""open"":""08:00"",""close"":""17:00"",""closed"":false},""saturday"":{""open"":""08:00"",""close"":""15:00"",""closed"":false},""sunday"":{""open"":"""",""close"":"""",""closed"":true},""holidayNote"":""Closed on regular national holidays""}',
                        25.0,
                        '[""Installation"",""Cleaning"",""Repair"",""Maintenance contracts"",""Parts sales""]',
                        1,
                        'Dedicated client parking bay in front of service facility.',
                        'AeroFlow HVAC Services Philippines Inc.', 'SEC-CS2024-91028'
                    );";
                await using var insertStoreCmd = db.CreateCommand(conn, insertStoreSql);
                await insertStoreCmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Migrates service_requests table to fix outdated CHECK constraint on status.
        /// Old constraint: status IN ('Pending','Assigned','In Progress','Resolved','Cancelled')
        /// New constraint: status IN ('Open','Assigned','In Progress','Completed')
        /// </summary>
        private static async Task MigrateServiceRequestsTableAsync(DbConnection conn, MyCon db)
        {
            try
            {
                // Check if migration is needed by trying an INSERT with status='Open'
                const string checkSql = "SELECT sql FROM sqlite_master WHERE type='table' AND name='service_requests'";
                string tableSql = string.Empty;
                await using (var checkCmd = db.CreateCommand(conn, checkSql))
                await using (var reader = await checkCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) tableSql = reader.GetString(0) ?? string.Empty;
                }

                // If old constraint still present (check for 'Pending' or 'Resolved'), migrate
                if (tableSql.Contains("'Pending'") || tableSql.Contains("'Resolved'"))
                {
                    const string migrationSql = @"
PRAGMA foreign_keys = OFF;
BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS service_requests_new (
    request_id INTEGER PRIMARY KEY AUTOINCREMENT,
    customer_id INTEGER NOT NULL,
    unit_id INTEGER NOT NULL,
    issue_description TEXT NOT NULL,
    priority TEXT DEFAULT 'Medium' CHECK(priority IN ('High','Medium','Low')),
    status TEXT DEFAULT 'Open' CHECK(status IN ('Open','Assigned','In Progress','Completed')),
    assigned_technician INTEGER,
    request_date DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (customer_id) REFERENCES customers(customer_id),
    FOREIGN KEY (unit_id) REFERENCES aircon_units(unit_id),
    FOREIGN KEY (assigned_technician) REFERENCES users(user_id)
);

INSERT INTO service_requests_new 
    (request_id, customer_id, unit_id, issue_description, priority, status, assigned_technician, request_date)
SELECT 
    request_id, customer_id, unit_id, issue_description,
    CASE priority WHEN 'Urgent' THEN 'High' ELSE COALESCE(priority, 'Medium') END,
    CASE status 
        WHEN 'Pending' THEN 'Open'
        WHEN 'Resolved' THEN 'Completed'
        WHEN 'Cancelled' THEN 'Completed'
        ELSE COALESCE(status, 'Open')
    END,
    assigned_technician,
    COALESCE(request_date, CURRENT_DATE)
FROM service_requests;

DROP TABLE service_requests;
ALTER TABLE service_requests_new RENAME TO service_requests;

COMMIT;
PRAGMA foreign_keys = ON;";
                    await using var migCmd = db.CreateCommand(conn, migrationSql);
                    await migCmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Migration] service_requests migration skipped: {ex.Message}");
            }
        }

        /// <summary>
        /// Migrates maintenance_schedule table to fix column name (scheduled_date -> schedule_date)
        /// and fix the status CHECK constraint.
        /// </summary>
        private static async Task MigrateMaintenanceScheduleTableAsync(DbConnection conn, MyCon db)
        {
            try
            {
                const string checkSql = "SELECT sql FROM sqlite_master WHERE type='table' AND name='maintenance_schedule'";
                string tableSql = string.Empty;
                await using (var checkCmd = db.CreateCommand(conn, checkSql))
                await using (var reader = await checkCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) tableSql = reader.GetString(0) ?? string.Empty;
                }

                // Check if old column name exists (scheduled_date) or outdated status CHECK constraint ('Pending')
                bool needsMigration = tableSql.Contains("scheduled_date") || tableSql.Contains("'Pending'");

                if (needsMigration)
                {
                    const string migrationSql = @"
PRAGMA foreign_keys = OFF;
BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS maintenance_schedule_new (
    schedule_id INTEGER PRIMARY KEY AUTOINCREMENT,
    customer_id INTEGER NOT NULL,
    unit_id INTEGER NOT NULL,
    technician_id INTEGER NOT NULL,
    schedule_date DATE NOT NULL,
    schedule_time TIME NOT NULL,
    status TEXT DEFAULT 'Scheduled' CHECK(status IN ('Scheduled','Completed','Cancelled')),
    remarks TEXT,
    FOREIGN KEY (customer_id) REFERENCES customers(customer_id),
    FOREIGN KEY (unit_id) REFERENCES aircon_units(unit_id),
    FOREIGN KEY (technician_id) REFERENCES users(user_id)
);

INSERT INTO maintenance_schedule_new
    (schedule_id, customer_id, unit_id, technician_id, schedule_date, schedule_time, status, remarks)
SELECT
    schedule_id, customer_id, unit_id, technician_id,
    COALESCE(schedule_date, scheduled_date, CURRENT_DATE),
    COALESCE(schedule_time, scheduled_time, '09:00'),
    CASE status 
        WHEN 'Pending' THEN 'Scheduled'
        WHEN 'In Progress' THEN 'Scheduled'
        ELSE COALESCE(status, 'Scheduled')
    END,
    COALESCE(remarks, notes)
FROM maintenance_schedule;

DROP TABLE maintenance_schedule;
ALTER TABLE maintenance_schedule_new RENAME TO maintenance_schedule;

COMMIT;
PRAGMA foreign_keys = ON;";
                    await using var migCmd = db.CreateCommand(conn, migrationSql);
                    await migCmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Migration] maintenance_schedule migration skipped: {ex.Message}");
            }
        }

        /// <summary>
        /// Migrates maintenance_records table to remove old NOT NULL constraints on service_date.
        /// </summary>
        private static async Task MigrateMaintenanceRecordsTableAsync(DbConnection conn, MyCon db)
        {
            try
            {
                const string checkSql = "SELECT sql FROM sqlite_master WHERE type='table' AND name='maintenance_records'";
                string tableSql = string.Empty;
                await using (var checkCmd = db.CreateCommand(conn, checkSql))
                await using (var reader = await checkCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync()) tableSql = reader.GetString(0) ?? string.Empty;
                }

                if (tableSql.Contains("service_date"))
                {
                    const string migrationSql = @"
PRAGMA foreign_keys = OFF;
BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS maintenance_records_new (
    record_id INTEGER PRIMARY KEY AUTOINCREMENT,
    schedule_id INTEGER NOT NULL UNIQUE,
    work_performed TEXT,
    parts_replaced TEXT,
    refrigerant_added TEXT,
    technician_notes TEXT,
    next_service_date DATE,
    completed_date DATE,
    FOREIGN KEY (schedule_id) REFERENCES maintenance_schedule(schedule_id) ON DELETE CASCADE
);

INSERT INTO maintenance_records_new
    (record_id, schedule_id, work_performed, parts_replaced, refrigerant_added, technician_notes, next_service_date, completed_date)
SELECT
    record_id, schedule_id, COALESCE(work_performed, ''), parts_replaced, refrigerant_added,
    COALESCE(technician_notes, findings), next_service_date, COALESCE(completed_date, service_date, CURRENT_DATE)
FROM maintenance_records;

DROP TABLE maintenance_records;
ALTER TABLE maintenance_records_new RENAME TO maintenance_records;

COMMIT;
PRAGMA foreign_keys = ON;";
                    await using var migCmd = db.CreateCommand(conn, migrationSql);
                    await migCmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Migration] maintenance_records migration skipped: {ex.Message}");
            }
        }

        private static async Task EnsureColumnExistsAsync(DbConnection conn, MyCon db, string table, string column, string columnDef)
        {
            try
            {
                string sql = $"ALTER TABLE {table} ADD COLUMN {column} {columnDef}";
                await using var cmd = db.CreateCommand(conn, sql);
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // Column already exists, ignore
            }
        }
    }
}
