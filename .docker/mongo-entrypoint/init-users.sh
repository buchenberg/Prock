if [ "$MONGODB_INITDB_ROOT_USERNAME" ] && [ "$MONGODB_INITDB_ROOT_PASSWORD" ]; then
  "${mongo[@]}" "$MONGODB_INITDB_DATABASE" <<-EOJS
  db.createUser({
     user: $(_js_escape "$MONGODB_INITDB_ROOT_USERNAME"),
     pwd: $(_js_escape "$MONGODB_INITDB_ROOT_PASSWORD"),
     roles: [ "readWrite", "dbAdmin" ]
     })
EOJS
fi

echo ======================================================
echo created $MONGODB_INITDB_ROOT_USERNAME in database $MONGODB_INITDB_DATABASE
echo ======================================================