program compthreedig;
var 
  a,b,c:integer;
begin
  a := 1;
  b := 4;
  c := 2;
  if (a > b) and (a > c) then 
    write(a)
  else
    if (b > a) and (b > c) then 
      write(b)
    else
      write(c);
end.