{ ------------------------------------------------------------------------ }
{ решение уравнений вида:                                                  }
{ |a1*x + b1*y + c1*z = d1|                                                }
{ |a2*x + b2*y + c2*z = d2|                                                }
{ |a3*x + b3*y + c3*z = d3|                                                }
{                                                                          }
{ метод решения:                                                           }
{     |d1 b1 c1|       |a1 d1 c1|       |a1 b1 d1|                         }
{     |d2 b2 c2|       |a2 d2 c2|       |a2 b2 d2|                         }
{     |d3 b3 c3|       |a3 d3 c3|       |a3 b3 d3|                         }
{ x = ----------   y = ----------   z = ----------                         }
{     |a1 b1 c1|       |a1 b1 c1|       |a1 b1 c1|                         }
{     |a2 b2 c2|       |a2 b2 c2|       |a2 b2 c2|                         }
{     |a3 b3 c3|       |a3 b3 c3|       |a3 b3 c3|                         }
{                                                                          }
{ выражаем определители третьего порядка:                                  }
{ e  := (a1*b2*c3+b1*c2*a3+c1*a2*b3-a3*b2*c1-b3*c2*a1-c3*a2*b1);           }
{ ex := (d1*b2*c3+b1*c2*d3+c1*d2*b3-d3*b2*c1-b3*c2*d1-c3*d2*b1);           }
{ ey := (a1*d2*c3+d1*c2*a3+c1*a2*d3-a3*d2*c1-d3*c2*a1-c3*a2*d1);           }
{ ez := (a1*b2*d3+b1*d2*a3+d1*a2*b3-a3*b2*d1-b3*d2*a1-d3*a2*b1);           }
{ x = ex/e                                                                 }
{ y = ey/e                                                                 }
{ z = ez/e                                                                 }
{ ------------------------------------------------------------------------ }
program equationsystem;
var a1,a2,a3,b1,b2,b3,c1,c2,c3,d1,d2,d3,x,y,z,e,ex,ey,ez:real;
begin
  a1 := 1; b1:=1; c1:=2; d1:=-1;
  a2 := 2; b2:=-1; c2:=2; d2:=-4;
  a3 := 4; b3:=1; c3:=4; d3:=-2;
  e  := (a1*b2*c3+b1*c2*a3+c1*a2*b3-a3*b2*c1-b3*c2*a1-c3*a2*b1);
  ex := (d1*b2*c3+b1*c2*d3+c1*d2*b3-d3*b2*c1-b3*c2*d1-c3*d2*b1);
  ey := (a1*d2*c3+d1*c2*a3+c1*a2*d3-a3*d2*c1-d3*c2*a1-c3*a2*d1);
  ez := (a1*b2*d3+b1*d2*a3+d1*a2*b3-a3*b2*d1-b3*d2*a1-d3*a2*b1);
  if ( e=0 ) and ( (ex=0) or (ey=0) or (ez=0) ) then
    writeln('бесконечное множество решений')
  else if ( e<>0 ) and ( (ex=0) or (ey=0) or (ez=0) ) then
    writeln('нет решений')
  else begin
    x:=ex/e; y:=ey/e; z:=ez/e;
    write('x = ');
    writeln (x);
    write('y = ');
    writeln (y);
    write('z = ');
    writeln (z);
 end;
end.