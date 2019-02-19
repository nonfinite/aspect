create table Tag
( id   integer not null constraint PK_Tag primary key autoincrement
, name text    not null constraint UQ_Tag_name unique
);

create table FileTag
( file_id integer not null constraint FK_FileTag_File references File(id) on delete cascade
, tag_id  integer not null constraint FK_FileTag_Tag  references Tag(id)  on delete cascade
, constraint PK_FileTag primary key (file_id, tag_id)
);
