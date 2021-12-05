program prog1;
var a, b, c, e: integer;
    a, b, d, superExpr: real;
    c: integer;
    s: string;
    bool: bolean;
begin
    a := -13;
    b := 0.3;
    
    abcd := 'abcd';
    f := 'abcd' + 1;
    
    if (a > 13) and (a + b < 13.03) then
        c := (13 - 9) / 2;
    
    if ('abc' > 'bcd') then e := 13 + 0.5;
    
    while a do begin
        a := a + 1;
        e := e / 2;
    end;
    
    superExpr := a * a * a - 3 * a * a * b + 3 * a * b * b - b * b * b;
    e := (a + b) * (a * a + a * b + b * b) div superExpr;
    
    s := -'a+b' + (s - 'a');
    
    while not a do c:=c+1;
    
    while not true do c:=c-2;
end.