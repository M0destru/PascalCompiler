program logicfunc;
var a, b, c, F:boolean;
begin
  a:=false; b:=true; c:=true;
  F := (not a) and (b or c);
  writeln (f);
  a:=true; b:=true; c:= false;
  F := (not a) and (b or c);
  writeln (F);
  a:=false; b:=false; c:= false;
  F := (not a) and (b or c);
  writeln (F);
end.