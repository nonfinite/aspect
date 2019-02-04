create table File
( id     integer not null constraint PK_File primary key autoincrement
, name   text    not null
, rating integer     null
, constraint UQ_File_name UNIQUE (name)
);
