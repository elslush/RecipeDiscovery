import re
import decimal
import pyodbc
from tokenize_training_dataset import dataset
from evaluate import predict

def _parseNumbers(s):
    """
    Parses a string that represents a number into a decimal data type so that
    we can match the quantity field in the db with the quantity that appears
    in the display name. Rounds the result to 2 places.
    """
    ss = re.sub(r'\$', " ", s)

    m3 = re.match('^\d+$', ss)
    if m3 is not None:
        return decimal.Decimal(round(float(ss), 2))

    m1 = re.match(r'(\d+)\s+(\d)/(\d)', ss)
    if m1 is not None:
        num = int(m1.group(1)) + (float(m1.group(2)) / float(m1.group(3)))
        return decimal.Decimal(str(round(num, 2)))

    m2 = re.match(r'^(\d)/(\d)$', ss)
    if m2 is not None:
        num = float(m2.group(1)) / float(m2.group(2))
        return decimal.Decimal(str(round(num, 2)))

    return None

ROW_CHUNK = 50
cnxn = pyodbc.connect('DRIVER={ODBC Driver 17 for SQL Server};SERVER=(local);DATABASE=FoodRecipes;Trusted_Connection=yes;')
insert_cnxn = pyodbc.connect('DRIVER={ODBC Driver 17 for SQL Server};SERVER=(local);DATABASE=FoodRecipes;Trusted_Connection=yes;', autocommit=False)
cursor = cnxn.cursor()
insert_cursor = insert_cnxn.cursor()

cursor.execute("SELECT IngredientID, Description FROM Data.Ingredients WHERE NAME IS NULL;")

rows = cursor.fetchmany(ROW_CHUNK)
i = 0
while rows:
    predictions = predict(rows)
    
    for result in predictions:
        names = []
        quantity = None
        units = []
        comments = []
        others = []
        
        for word, prediction in zip(result[0][1].split(), result[1]):
            match prediction:
                case 'B-NAME' | 'I-NAME':
                    names.append(word)
                case 'B-QUANTITY' | 'I-QUANTITY':
                    num = _parseNumbers(word)
                    if num is not None:
                        quantity = num
                case 'B-UNIT' | 'I-UNIT':
                    units.append(word)
                case 'B-COMMENT' | 'I-COMMENT':
                    comments.append(word)
                case _:
                    others.append(word)

        insert_cursor.execute("UPDATE Data.Ingredients SET Name = ?, Quantity = ?, Unit = ?, Comment = ?, other = ?  WHERE IngredientID = ?;",
            ' '.join(names),
            quantity,
            ' '.join(units),
            ' '.join(comments),
            ' '.join(others),
            result[0][0]
        )
        i += 1
    print("completed: " + str(i))
    insert_cnxn.commit()
    rows = cursor.fetchmany(ROW_CHUNK)
        

insert_cnxn.commit()
insert_cnxn.close()
cnxn.close()