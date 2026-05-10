ALTER TABLE users
    MODIFY notifyondeviation BIT NOT NULL DEFAULT 1,
    MODIFY autodeterminestrategy BIT NOT NULL DEFAULT 1,
    MODIFY emailcooldownenabled BIT NOT NULL DEFAULT 0,
    MODIFY emailcooldownhours INT NOT NULL DEFAULT 24,
    MODIFY globaltolerancepercent DECIMAL(5,2) NULL DEFAULT NULL;

DELETE from users;
INSERT INTO users (
    email,
    hashedpassword,
    role,
    twofactorenabled,
    EmailConfirmed,
    MinimumSwappInUSD,
    NotifyOnDeviation,
    UserProfileImage
)
VALUES (
           'user@example.com',
           '$2a$12$ScfwdXZPcmOFvtfDQJzneuGko2C61TOb1uCQ3YJ7C9wE6JheEq.Am',  -- test123
           0,
           0,
           true,
           5.0,
           true,
           ''
       );
INSERT INTO users (
    email,
    hashedpassword,
    role,
    twofactorenabled,
    EmailConfirmed,
    MinimumSwappInUSD,
    NotifyOnDeviation,
    UserProfileImage
)
VALUES (
           'admin@example.com',
           '$2a$12$ScfwdXZPcmOFvtfDQJzneuGko2C61TOb1uCQ3YJ7C9wE6JheEq.Am', -- test123
           1,         -- Admin
           false,     -- 2FA deaktiviert
           true,      -- Email bestätigt
           5.0,
           true,
           ''
       );