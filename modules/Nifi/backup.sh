

# copy nifi files to our local shared folder :c/config/local/nifi for future reuse
# remember to back up existing files before running this
rm -R /config/local/nifi/

mkdir /config/local/nifi/
mkdir /config/local/nifi/conf
mkdir /config/local/nifi/content_repository
mkdir /config/local/nifi/database_repository
mkdir /config/local/nifi/flowfile_repository
mkdir /config/local/nifi/provenance_repository

cd /opt/nifi/nifi-current/conf
cp -R * /config/local/nifi/conf

cd /opt/nifi/nifi-current/content_repository
cp -R * /config/local/nifi/content_repository

cd /opt/nifi/nifi-current/database_repository
cp -R * /config/local/nifi/database_repository

cd /opt/nifi/nifi-current/flowfile_repository
cp -R * /config/local/nifi/flowfile_repository

cd /opt/nifi/nifi-current/provenance_repository
cp -R * /config/local/nifi/provenance_repository