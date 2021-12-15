program fact;
var i, num, fact: integer;
begin
    num := 8;
    fact := 1;
    i := 2;
    while (i <= num) do
    begin
        fact := fact * i;
        i := i + 1;
    end;
    write ('Факториал числа ');
    writeln (num);
    writeln (fact);
    
end.