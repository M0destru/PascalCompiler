program test2;
var e: real;
    a, b, c: integer;
    str: string;
begin
    a := -10;
    e := 5.5;
    
    while c > 1 do begin
        c := c - 1 ;
    end;
    
    c := c + 25 * 10 - (- 1 + 5 div (2 * 6));

    e:=+5.6;
    
    if c >= 0 then 
    begin
        while a < 0 do
            a := a + 1;
    end;
        
    if c + e < 0 then 
    begin
        str := 'str1';
    end
    else 
    begin
        e := 0;
        str := 'str2';
    end;    
end.