USE [TPCH]

-- BEFORE BACKUP, AFTER ENABLING CDC
insert into dbo.REGION
	([R_REGIONKEY], [R_NAME], [R_COMMENT])
values 
	(99, 'Unkown', 'Unkown')
;

insert into dbo.NATION 
	([N_NATIONKEY], [N_NAME], [N_REGIONKEY], [N_COMMENT])
values 
	(99, 'Unkown', 99, 'Unkown')
;

