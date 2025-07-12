-- Add PushSubscriptions table for web push notifications
IF OBJECT_ID('dbo.PushSubscriptions', 'U') IS NOT NULL DROP TABLE dbo.PushSubscriptions;

CREATE TABLE dbo.PushSubscriptions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL,
    Endpoint NVARCHAR(MAX) NOT NULL,
    P256Dh NVARCHAR(MAX) NOT NULL,
    Auth NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Create unique index on Endpoint
CREATE UNIQUE INDEX IX_PushSubscriptions_Endpoint ON dbo.PushSubscriptions (Endpoint) WHERE IsActive = 1;

-- Insert sample push subscription for admin (you can remove this in production)
-- Note: These are placeholder values and should be replaced with real subscription data
INSERT INTO dbo.PushSubscriptions (UserId, Endpoint, P256Dh, Auth, IsActive) VALUES
('admin-user-guid', 'https://fcm.googleapis.com/fcm/send/sample-endpoint', 'sample-p256dh-key', 'sample-auth-key', 1);

PRINT 'PushSubscriptions table created successfully!'; 