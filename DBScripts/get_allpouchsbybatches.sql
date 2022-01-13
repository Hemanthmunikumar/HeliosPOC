-- FUNCTION: public.get_allpouchsbybatches(character varying, character varying)

-- DROP FUNCTION public.get_allpouchsbybatches(character varying, character varying);

CREATE OR REPLACE FUNCTION public.get_allpouchsbybatches(
	p_drugnames character varying,
	p_batches character varying)
    RETURNS TABLE(r_id integer, r_pouchid character varying, r_fkbatch integer, r_pathyear integer, r_pathmonth integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
    
AS $BODY$
BEGIN
return query
select

tblpouch.id,
tblpouch.pouchid,
tblpouch.fkbatch,
tblpouch.pathyear,
tblpouch.pathmonth
from tblpouch 

left join tblbatch on tblbatch.id = tblpouch.fkbatch 

left join
(
select tblpillinpouch.fkpouch, count(*)
from tblpillinpouch
group by fkpouch
) totalpips on totalpips.fkpouch = tblpouch.id 

left join
(
	select tblpillinpouch.fkpouch pouchid, count(*)
	from tblpillinpouch
	left join
(
select fkpillinpouch, count(*)
from tblpillfound
--where w>0 and h>0
group by fkpillinpouch
) pillsfound on pillsfound.fkpillinpouch=tblpillinpouch.id

	where amount % 1 = 0
	and amount = coalesce(pillsfound.count, 0)
	group by pouchid
) usablepips on usablepips.pouchid = tblpouch.id 

left join
(
	select tblpillinpouch.fkpouch, count(*)
	from tblpillinpouch
	left join tblmedication on tblmedication.id = tblpillinpouch.fkmedication
	where tblmedication.uniqueidentifier not in (p_drugnames)
	group by fkpouch
) trainablemeds on trainablemeds.fkpouch = tblpouch.id 

where coalesce(totalpips.count,0) > 0 
and tblpouch.ok = TRUE 
and tblpouch.repaired = FALSE 
and tblbatch.isdone = TRUE 
--and coalesce(totalpips.count,0) = coalesce(usablepips.count,0) 
and tblpouch.csvdeposited = FALSE 
and trainablemeds.count > 0 
and ( (tblpouch.colorimagedeposited = TRUE and tblpouch.monoimagedeposited = TRUE)
	 or tblpouch.fkbatch = any(string_to_array(p_batches, ',')::int[])
	); 
END;
$BODY$;

ALTER FUNCTION public.get_allpouchsbybatches(character varying, character varying)
    OWNER TO postgres;
