CREATE TABLE dbo.WorkRequests
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkRequests PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    ClientName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(2000) NULL,
    Priority NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_WorkRequests_Status DEFAULT 'New',
    DueDate DATETIME2 NOT NULL,
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_WorkRequests_CreatedDate DEFAULT SYSUTCDATETIME(),
    UpdatedDate DATETIME2 NOT NULL CONSTRAINT DF_WorkRequests_UpdatedDate DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_WorkRequests_Priority CHECK (Priority IN ('Low', 'Medium', 'High')),
    CONSTRAINT CK_WorkRequests_Status CHECK (Status IN ('New', 'InProgress', 'Blocked', 'Completed'))
);

CREATE TABLE dbo.WorkRequestNotes
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkRequestNotes PRIMARY KEY,
    WorkRequestId INT NOT NULL,
    [Text] NVARCHAR(1000) NOT NULL,
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_WorkRequestNotes_CreatedDate DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_WorkRequestNotes_WorkRequests FOREIGN KEY (WorkRequestId)
        REFERENCES dbo.WorkRequests(Id) ON DELETE CASCADE
);

-- Helps queries that filter by status and sort by creation date.
CREATE INDEX IX_WorkRequests_Status_CreatedDate
    ON dbo.WorkRequests(Status, CreatedDate DESC);

-- Helps search/lookup by client; title search may still need full-text search for large datasets.
CREATE INDEX IX_WorkRequests_ClientName
    ON dbo.WorkRequests(ClientName);

-- Helps retrieving notes for a given work request.
CREATE INDEX IX_WorkRequestNotes_WorkRequestId
    ON dbo.WorkRequestNotes(WorkRequestId);
