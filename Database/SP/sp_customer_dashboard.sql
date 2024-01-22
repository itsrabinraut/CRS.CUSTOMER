USE [CRS]
GO

/****** Object:  StoredProcedure [dbo].[sp_customer_dashboard_locationlist]    Script Date: 10/20/2023 9:59:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Paras Maharjan>
-- Create date: <2023/10/20>
-- Description:	<for selecting dashboard items for customer>
-- =============================================
ALTER PROCEDURE [dbo].[sp_customer_dashboard] @Flag VARCHAR(10) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF ISNULL(@Flag, '') = 'gll' --get location list
	BEGIN
		SELECT LocationId AS LocationId
			,LocationName AS LocationName
			,LocationImage AS LocationImage
			,LocationURL AS LocationURL
		FROM dbo.tbl_location WITH (NOLOCK)
		WHERE ISNULL(STATUS, '') = 'B';
	END;

	-- GET BANNER/PROMOTIONAL IMAGE LISTS
	IF @Flag = 'bl'
	BEGIN
		SELECT Sno BannerId
			,Title BannerName
			,ImgPath BannerImage
		FROM tbl_promotional_images WITH (NOLOCK)
		WHERE isDeleted <> 1
	END
END;
