program logic;
var x, y: real;
  b1, b2, b3, b4, b5, b6: boolean;
begin
  x := 7.5;
  y := 4.0;
  
  b1 := x < y;
  b2 := x <= y;
  b3 := x > y;
  b4 := x >= y;
  b5 := x = y;
  b6 := x <> y;
  
  write ('Первое меньше второго: ');
  writeln (b1);
  write ('Первое меньше или равно второму: ');
  writeln (b2);
  write ('Первое больше второго: ');
  writeln (b3);
  write ('Первое больше или равно второму: ');
  writeln (b4);
  write ('Первое равно второму: ');
  writeln (b5);
  write ('Первое не равно второму: ');
  writeln (b6);
end.