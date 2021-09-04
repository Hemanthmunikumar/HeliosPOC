-- FUNCTION: public.get_pouch_details(character varying, integer)

-- DROP FUNCTION public.get_pouch_details(character varying, integer);

CREATE OR REPLACE FUNCTION public.get_pouch_details(
	p_pouchid character varying,
	p_batchid integer)
    RETURNS TABLE(r_id integer, r_pouchid character varying, r_tracepacketid integer, r_concat text, r_to_char text, r_intakedate character varying, r_intaketime character varying, r_repaired boolean, r_string_agg text, r_ok boolean, r_situationnew integer, r_randfrac real) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
    
AS $BODY$
BEGIN
return query
select

p.id,
pouchid,
tracepacketid,
concat(size,'mm'),
to_char(p.createdate,'YYYY-MM-DD HH24:MI:SS.ms'),
intakedate,
intaketime,
repaired,
string_agg(rt.description, ', '),
ok,
situationnew,
randfrac

from tblpouch p

left join tblbatch b on b.id = p.fkbatch
left join tblpouchrepair r on r.fkpouch = p.id
left join tblrepairtype rt on rt.id = r.fkrepairtype

where p.emptypacket = FALSE
and b.isdone = TRUE
and p.jsondeposited = FALSE
--and (p.randfrac <0.10 or p.repaired=true)
and ( (p.colorimagedeposited = TRUE and p.monoimagedeposited = TRUE) or p.fkbatch in (p_batchid))
and pouchid=p_pouchid
group by p.id;
UPDATE tblpouch SET jsondeposited=TRUE,csvdeposited=TRUE WHERE pouchid=p_pouchid;
END;
$BODY$;

ALTER FUNCTION public.get_pouch_details(character varying, integer)
    OWNER TO postgres;
