DECLARE @UserID UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE UserName ='SuperKicks'),@RoleID UNIQUEIDENTIFIER = (SELECT Id FROM ROLES WHERE NAME='User')
UPDATE Users SET PasswordHash = 'oMiiFV6QuxpUJOvqJCeeEK17mns1be0y4/jznXkizIvborNnLB8dkFGvi9b5lAOa' , UserName = 'SuperKicks' , 
NormalizedUserName = 'SUPERKICKS' , Email = 'superKicks@gmail.com' ,  NormalizedEmail = 'SUPERKICKS@GMAIL.COM' 
WHERE Id = @UserID

IF(@RoleID IS NULL)
BEGIN
	INSERT Roles(Id,Name,CreatedBy,CraetedDateTime) VALUES ((SELECT NEWID()),@RoleID,1,(SELECT SYSDATETIMEOFFSET()))
END

INSERT UserRoles (UserId,RoleId,CreatedBy,CraetedDateTime) VALUES (@UserID,@RoleID,1,(SELECT SYSDATETIMEOFFSET()))


--"userName": "SuperKicks",
--"password": "SuperKicks@1234",
--"email": "superkicks@gmail.com",
--"phoneNumber": "1234567890"