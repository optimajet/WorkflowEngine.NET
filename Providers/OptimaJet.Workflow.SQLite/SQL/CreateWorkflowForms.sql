CREATE TABLE IF NOT EXISTS WorkflowForm (
                                            Id TEXT NOT NULL,
                                            Name TEXT NOT NULL,
                                            Version INTEGER NOT NULL,
                                            CreationDate INTEGER NOT NULL DEFAULT ((CAST(strftime('%s', 'now') AS INTEGER) * 10000000) + 621355968000000000),
                                            UpdatedDate INTEGER NOT NULL DEFAULT ((CAST(strftime('%s', 'now') AS INTEGER) * 10000000) + 621355968000000000),
                                            Definition TEXT NOT NULL,
                                            Lock INTEGER NOT NULL,
                                            PRIMARY KEY (Id),
    UNIQUE (Name, Version)
    );