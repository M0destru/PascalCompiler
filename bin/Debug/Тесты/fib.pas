program fib;
var
    i, n, s, a, a1: integer;
begin
    n := 11;
    i := 1;
    writeln('Числа Фибоначчи');
    while i <= n do
    begin
        if (i = 1) or (i = 2) then
            s := 1
        else
            s := a + a1;
        a := a1;
        a1 := s;    
        writeln(s);
        i := i + 1;
    end;  
end. 