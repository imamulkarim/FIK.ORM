CREATE TABLE public."Item"
(
    "Id" bigint NOT NULL ,
    "Name" character varying(50),
    "Price" money,
    PRIMARY KEY ("Id") 
);

ALTER TABLE IF EXISTS public."Item"
    OWNER to postgres;



CREATE TABLE public."Sale"
(
    "Id" bigint,
    "ItemId" bigint,
    "Quantity" numeric(18, 4),
    "TotalPrice" numeric(18, 4),
    PRIMARY KEY ("Id"),
    CONSTRAINT fk_item_sale FOREIGN KEY ("ItemId")
        REFERENCES public."Item" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS public."Sale"
    OWNER to postgres;



CREATE TABLE "dboInvn"."Inventory"
(
    "Id" bigint NOT NULL,
    "ItemId" bigint,
    "Quantity" numeric(18, 4),
    PRIMARY KEY ("Id"),
    CONSTRAINT fk_item_inventory FOREIGN KEY ("ItemId")
        REFERENCES public."Item" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE IF EXISTS "dboInvn"."Inventory"
    OWNER to postgres;


ALTER TABLE IF EXISTS "dboInvn"."Inventory"
    ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 );



CREATE TABLE public."ItemBlob"
(
    "Id" bigint NOT NULL, --GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 ),
    "BlobData" bytea,
    PRIMARY KEY ("Id")
);

ALTER TABLE IF EXISTS public."ItemBlob"
    OWNER to postgres;