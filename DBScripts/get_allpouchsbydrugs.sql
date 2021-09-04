-- FUNCTION: public.get_allpouchsbydrugs(character varying)

-- DROP FUNCTION public.get_allpouchsbydrugs(character varying);

CREATE OR REPLACE FUNCTION public.get_allpouchsbydrugs(
	p_drugnames character varying)
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

left join (select tblpillinpouch.fkpouch, count(*) from tblpillinpouch group by fkpouch) totalpips on totalpips.fkpouch = tblpouch.id

left join
(
	select tblpillinpouch.fkpouch pouchid, count(*)

	from tblpillinpouch

	left join (select fkpillinpouch, count(*) from tblpillfound where x>0 and y>0 group by fkpillinpouch) pillsfound on pillsfound.fkpillinpouch=tblpillinpouch.id

	where --amount \ 1 = 0 and
	amount = coalesce(pillsfound.count, 0)

	group by pouchid
) usablepips on usablepips.pouchid = tblpouch.id

left join
(
	select tblpillinpouch.fkpouch

	from tblpillinpouch

	left join tblmedication on tblmedication.id = tblpillinpouch.fkmedication
	where tblmedication.uniqueidentifier not in (p_drugnames)

) trainablemeds on trainablemeds.fkpouch = tblpouch.id

where coalesce(totalpips.count,0) > 0
--and tblpouch.ok = TRUE
and tblpouch.repaired = FALSE
and tblbatch.isdone = TRUE
--and coalesce(totalpips.count,0) = coalesce(usablepips.count,0)
and tblpouch.csvdeposited = FALSE
--and trainablemeds.count > 0
--and ( (tblpouch.colorimagedeposited = TRUE and tblpouch.monoimagedeposited = TRUE) or tblpouch.fkbatch in (p_batchid));
and ( (tblpouch.colorimagedeposited = TRUE and tblpouch.monoimagedeposited = TRUE));
END;
$BODY$;

ALTER FUNCTION public.get_allpouchsbydrugs(character varying)
    OWNER TO postgres;
