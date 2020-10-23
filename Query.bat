select AR.DESCRIPCION, AR.CODIGOPARTICULAR, AR.CODIGOUNIDADMEDIDA, AR.CODIGORUBRO, sp.stockremanente,
iif(AR.COEFICIENTE = 1, (AR.PRECIOVENTA1-AR.precioventa1*0.28) * 1.21, iif(AR.COEFICIENTE = 0.5, (AR.PRECIOVENTA1-AR.precioventa1*0.28) * 1.105, 0)) as PRECIOLISTA1,AR.COEFICIENTEVARIACION
 from SP_STOCK_REM SP
inner join articulos ar on ar.codigoarticulo=sp.codigo
where AR.MUESTRAWEB=1



