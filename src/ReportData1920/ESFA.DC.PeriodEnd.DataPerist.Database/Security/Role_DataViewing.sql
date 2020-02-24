
CREATE ROLE [DataViewing] AUTHORIZATION [dbo]
GO

-- Grant access rights to a specific schema in the database
GRANT 
	REFERENCES, 
	SELECT, 
	VIEW DEFINITION 
ON SCHEMA::[dbo]
	TO [DataViewing]
GO
