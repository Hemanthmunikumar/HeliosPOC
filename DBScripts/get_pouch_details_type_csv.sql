-- FUNCTION: public.get_pouch_details_type_csv(integer)

-- DROP FUNCTION public.get_pouch_details_type_csv(integer);

CREATE OR REPLACE FUNCTION public.get_pouch_details_type_csv(
	p_pouchid integer)
    RETURNS TABLE(relativedirpath text, filename text, class text, xmin integer, ymin integer, xmax integer, ymax integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
    
AS $BODY$
BEGIN
return query
select

concat(pathyear,'\',pathmonth,'\',pathfk,'\') relativedirpath,
concat(pouchid, '.jpg') filename, 

case
when uniqueidentifier like '%/_%' then 'partialTablet'
when (capsule or granular or transparent or transparentother) then 'capsule'
else 'tablet'
end as class, 

pillcoord.x - pillcoord.w/2 xmin,
pillcoord.y - pillcoord.h/2 ymin,
pillcoord.x + pillcoord.w/2 xmax,
pillcoord.y + pillcoord.h/2 ymax 

from tblpouch pouch

left join tblpillinpouch pill on pill.fkpouch = pouch.id
left join tblpillfound pillcoord on pillcoord.fkpillinpouch = pill.id
left join tblmedication med on med.id = pill.fkmedication 

where pouch.id = p_pouchid; 
END;
$BODY$;

ALTER FUNCTION public.get_pouch_details_type_csv(integer)
    OWNER TO postgres;
