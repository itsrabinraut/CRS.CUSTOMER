USE [CRS]
GO
/****** Object:  StoredProcedure [dbo].[sproc_customer_login_management]    Script Date: 10/28/2023 9:56:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROC [dbo].[sproc_customer_login_management] @Flag VARCHAR(10)
	,@LoginId VARCHAR(200) = NULL
	,@Password VARCHAR(16) = NULL
	,@ActionIP VARCHAR(50) = NULL
	,@ActionPlatform VARCHAR(10) = NULL
AS
DECLARE @MaxFailedLoginAttempt INT = 5
	,@Session VARCHAR(500);

BEGIN
	IF ISNULL(@Flag, '') = 'login'
	BEGIN
		IF NOT EXISTS (
				SELECT 'X'
				FROM dbo.tbl_customer a WITH (NOLOCK)
				INNER JOIN dbo.tbl_users b WITH (NOLOCK) ON b.AgentId = a.AgentId
					AND ISNULL(b.[Status], '') = 'A'
				WHERE b.LoginId = @LoginId
				)
		BEGIN
			SELECT 1 Code
				,'Customer is inactive' Message;

			RETURN;
		END;

		IF NOT EXISTS (
				SELECT 'X'
				FROM dbo.tbl_customer a WITH (NOLOCK)
				INNER JOIN dbo.tbl_users b WITH (NOLOCK) ON b.AgentId = a.AgentId
					AND ISNULL(b.[Status], '') = 'A'
				WHERE b.LoginId = @LoginId
					AND PWDCOMPARE(@Password, b.Password) = 1
				)
		BEGIN
			IF EXISTS (
					SELECT 'X'
					FROM dbo.tbl_customer a WITH (NOLOCK)
					INNER JOIN dbo.tbl_users b WITH (NOLOCK) ON b.AgentId = a.AgentId
						AND ISNULL(b.[Status], '') = 'A'
					WHERE b.LoginId = @LoginId
						AND ISNULL(b.FailedLoginAttempt, 0) = @MaxFailedLoginAttempt
					)
			BEGIN
				UPDATE dbo.tbl_users
				SET STATUS = 'B'
					,FailedLoginAttempt = 0
					,Session = NULL
					,ActionUser = @LoginId
					,ActionIP = @ActionIP
					,ActionPlatform = @ActionPlatform
					,ActionDate = GETDATE()
				WHERE LoginId = @LoginId
					AND [Status] = 'A';

				SELECT 1 Code
					,'Invalid credentials. User is blocked' Message;

				RETURN;
			END;
		END;

		IF EXISTS (
				SELECT 'X'
				FROM dbo.tbl_customer a WITH (NOLOCK)
				INNER JOIN dbo.tbl_users b WITH (NOLOCK) ON b.AgentId = a.AgentId
					AND ISNULL(b.[Status], '') = 'A'
				WHERE b.LoginId = @LoginId
					AND PWDCOMPARE(@Password, b.Password) = 1
				)
		BEGIN
			SELECT @Session = NEWID();

			UPDATE dbo.tbl_users
			SET Session = @Session
				,FailedLoginAttempt = 0
				,ActionUser = @LoginId
				,ActionIP = @ActionIP
				,ActionPlatform = @ActionPlatform
				,ActionDate = GETDATE()
			WHERE LoginId = @LoginId
				AND [Status] = 'A';

			SELECT 0 Code
				,'Success' Message
				,a.AgentId
				,b.UserId
				,a.NickName
				,a.EmailAddress
				,a.ProfileImage
				,@Session AS SessionId
				,a.ActionDate AS ActionDate
			FROM dbo.tbl_customer a WITH (NOLOCK)
			INNER JOIN dbo.tbl_users b WITH (NOLOCK) ON b.AgentId = a.AgentId
				AND ISNULL(b.[Status], '') = 'A'
			WHERE b.LoginId = @LoginId
				AND PWDCOMPARE(@Password, b.Password) = 1;
		END;
		ELSE IF (
				SELECT b.FailedLoginAttempt
				FROM dbo.tbl_customer a WITH (NOLOCK)
				INNER JOIN dbo.tbl_users b WITH (NOLOCK) ON b.AgentId = a.AgentId
					AND ISNULL(b.[Status], '') = 'A'
				WHERE b.LoginId = @LoginId
				) = (@MaxFailedLoginAttempt - 1)
		BEGIN
			UPDATE dbo.tbl_users
			SET FailedLoginAttempt = ISNULL(FailedLoginAttempt, 0) + 1
				,ActionUser = @LoginId
				,ActionIP = @ActionIP
				,ActionPlatform = @ActionPlatform
				,ActionDate = GETDATE()
			WHERE LoginId = @LoginId;

			SELECT 1 Code
				,'Invalid credentials! <br/> Last attempt remaning.' Message;

			RETURN;
		END;
		ELSE
		BEGIN
			UPDATE dbo.tbl_users
			SET FailedLoginAttempt = ISNULL(FailedLoginAttempt, 0) + 1
				,ActionUser = @LoginId
				,ActionIP = @ActionIP
				,ActionPlatform = @ActionPlatform
				,ActionDate = GETDATE()
			WHERE LoginId = @LoginId;

			SELECT 1 Code
				,'Invalid credentials!' Message;

			RETURN;
		END;
	END;
END;
