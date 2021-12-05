program prog;
var m, n:integer;
    Y: real;
    flag: boolean;
begin
    m := 15;
    n := 45;
    Y := (m + n) / 12;
    while m <> n do
    begin
        if m > n then 
            m := m - n
        else 
            n := n - m;
    end;  
    if Y / 2 > m then
        flag := true;
    if flag then 
    begin
        Y := Y / 2;
        flag := not flag;
    end
    else
        Y := m div 2;
end.