import json
import logging
import os
import pyodbc
from datetime import datetime
from azure.functions import HttpRequest, HttpResponse

def get_db_connection():
    conn_str = os.environ['DATABASE_CONNECTION_STRING']
    # Parse the existing connection string for server, database, user, and password
    server = conn_str.split('Server=')[1].split(';')[0]
    database = conn_str.split('Initial Catalog=')[1].split(';')[0]
    user = conn_str.split('User ID=')[1].split(';')[0]
    password = conn_str.split('Password=')[1].split(';')[0]
    
    # Build a new connection string with explicit parameters
    new_conn_str = (
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"Server={server};"
        f"Database={database};"
        f"UID={user};"
        f"PWD={password};"
        "Encrypt=no"
    )
    return pyodbc.connect(new_conn_str)

async def main(req: HttpRequest) -> HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        if req.method == "GET":
            conn = get_db_connection()
            cursor = conn.cursor()
            cursor.execute("SELECT * FROM items")
            rows = cursor.fetchall()
            items = [{"id": row[0], "name": row[1], "created_at": row[2].isoformat()} for row in rows]
            return HttpResponse(
                json.dumps({"items": items}),
                mimetype="application/json"
            )

        elif req.method == "POST":
            req_body = req.get_json()
            if not req_body or 'name' not in req_body:
                return HttpResponse(
                    json.dumps({"error": "Please provide a name in the request body"}),
                    status_code=400,
                    mimetype="application/json"
                )

            conn = get_db_connection()
            cursor = conn.cursor()
            cursor.execute(
                "INSERT INTO items (name, created_at) VALUES (?, ?)",
                (req_body['name'], datetime.utcnow())
            )
            conn.commit()
            
            return HttpResponse(
                json.dumps({"message": "Item created successfully"}),
                status_code=201,
                mimetype="application/json"
            )

    except Exception as e:
        logging.error(f"Error: {str(e)}")
        return HttpResponse(
            json.dumps({"error": str(e)}),
            status_code=500,
            mimetype="application/json"
        )

    return HttpResponse(
        json.dumps({"error": "Method not allowed"}),
        status_code=405,
        mimetype="application/json"
    ) 