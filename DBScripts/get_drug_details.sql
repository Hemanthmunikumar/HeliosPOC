-- FUNCTION: public.get_drug_details(character varyin)

-- DROP FUNCTION public.get_drug_details(integer);

CREATE OR REPLACE FUNCTION public.get_drug_details(
	p_pouchid integer)
    RETURNS TABLE(R_drug_code character varying, R_generic_name character varying, R_drug_quantity integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
    
AS $BODY$
BEGIN
return query
select

uniqueidentifier,
description,
amount

from tblpillinpouch

left join tblmedication on tblmedication.id = tblpillinpouch.fkmedication

where fkpouch = p_pouchid

order by tblpillinpouch.id;

END;
$BODY$;

ALTER FUNCTION public.get_drug_details(integer)
    OWNER TO postgres;
