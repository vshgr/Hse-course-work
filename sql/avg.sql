SELECT * 
FROM (
    SELECT subs.Compound AS 'Формула вещества', 
           gap.SpaceGroup,
           gap.CrystalSystem,
           gap.Modification
    FROM  [420b97c17834].BandGap.dbo.Substances AS subs
    LEFT OUTER JOIN [420b97c17834].BandGap.dbo.Gap AS gap ON subs.SubstanceID = gap.SubstanceID
    GROUP BY subs.Compound, gap.SpaceGroup, gap.CrystalSystem, gap.Modification
) 
AS table1 FULL OUTER JOIN
(
    SELECT soed.Соединение,
           CAST(AVG(ad.[M1, 10-7 см2*cек/г]) AS decimal(12,2)) AS [AVG_M1],
           CAST(AVG(ad.[M2, 10-18 сек3/г]) AS decimal(12,2)) AS [AVG_M2],
           CAST(AVG(ad.[M3, 10-12 см*сек2/г]) AS decimal(12,2)) AS [AVG_M3],
           CAST(AVG(ad.[Длина волны, мкм]) AS decimal(12,2)) AS [AVG_wave_length],
           CAST(AVG(gg.[Верхняя граница области гомогенности]) AS decimal(12,2)) AS [AVG_upper],
           CAST(AVG(gg.[Нижняя граница области гомогенности]) AS decimal(12,2)) AS [AVG_lower],
           tudp.[Обозначение тангенса угла потерь],
           CAST(AVG(tudp.[Значение тангенса угла]) AS decimal(12,2)) AS 'Среднее значение тангенса угла',
           CAST(AVG(tudp.Погрешность) AS decimal(12,2)) AS [AVG_pogr],
           CAST(AVG(tk.[Температура, K]) AS decimal(12,2)) AS 'Температура Кюри',
           CAST(AVG(tk.[Погрешность, K]) AS decimal(12,2)) AS 'Погрешность Т Кюри',
           tk.[Тип фазового перехода]
    FROM   [420b97c17834].Crystal.dbo.Соединения AS soed
    FULL OUTER JOIN [420b97c17834].Crystal.dbo.[Акустооптическая добротность] AS ad ON soed.[Номер соединения] = ad.[Номер соединения]
    FULL OUTER JOIN [420b97c17834].Crystal.dbo.[Гомогенность элементов] AS gg ON soed.[Номер соединения] = gg.[Номер соединения]
    FULL OUTER JOIN [420b97c17834].Crystal.dbo.[Тангенс угла диэлектрических потерь] AS tudp ON soed.[Номер соединения] = tudp.[Номер соединения]
    FULL OUTER JOIN [420b97c17834].Crystal.dbo.[Температура Кюри] AS tk ON soed.[Номер соединения] = tk.[Номер соединения]
    GROUP BY soed.Соединение, tudp.[Обозначение тангенса угла потерь], tk.[Тип фазового перехода]
) 
AS table2 
ON table1.[Формула вещества] = table2.Соединение COLLATE SQL_Latin1_General_CP1251_CS_AS 
ORDER BY LEN(table1.[Формула вещества]), table1.[Формула вещества]








      
