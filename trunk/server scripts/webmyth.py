#!/usr/bin/python
# -*- coding: utf-8 -*-
# coding: utf-8
#
# webmyth.py
#
# Original script inspired by Kyle Stoneman from http://www.legatissimo.info/node/355
# Modified beyond recognition for use in WebMyth   http://code.google.com/p/webmyth/
#
# This file needs to be saved on a local webserver in the 'cgi-bin' folder
# On my system running Ubuntu 9.10 (Karmic) the file is saved in /usr/lib/cgi-bin/ and is accessable as http://my-server/cgi-bin/webmyth.py
# This files needs to be accesible without authentication on the webserver
#
# You will also need to have a working copy of python installed on the server
#
# This script replaces previous scripts used by webmyth (remote.py and webmyth-mysql.php)
#
# You need to setup this configuration for your specifc setup - you can find these values in /etc/mythtv/mysql.txt
#

myDBHostName = 'localhost'
myDBName = 'mythconverg'
myDBUserName = 'mythtv'
myDBPassword = 'mythtv'

#
# end of configuration
#

version = 10

import cgi
import cgitb
import time
import datetime
#import json
from simplejson import dumps
import string
import os
import sys
import base64
#cgitb.enable()
from telnetlib import Telnet
from MythTV import MythDB, MythBE, Frontend, MythVideo, MythXML, MythLog, MythError, Video, ftopen

import MySQLdb

#Try using settings file is possible - thanks to muhgatus for the code
mythtvMyConfig="/etc/mythtv/mysql.txt"

def loadConfig():
        """Load mysql configuration from MythTV mysql.txt settings file"""
        global myDBHostName, myDBName, myDBUserName, myDBPassword
        if os.path.exists(mythtvMyConfig):
                try:
                        f=open(mythtvMyConfig)
                except:
                        return
                for line in f:
                        key, value = line.strip().split('=')
                        if 'DBHostName' in key:
                                myDBHostName = value.strip()
                        if 'DBUserName' in key:
                                myDBUserName = value.strip()
                        if 'DBName' in key:
                                myDBName = value.strip()
                        if 'DBPassword' in key:
                                myDBPassword = value.strip()
                f.close()

loadConfig()

def repackDicts(datadict):
        d={}
        for key, value in datadict.items():
                if type(value) == datetime.datetime:
                        value = value.strftime('%Y-%m-%dT%H:%M:%S')
                if type(value) == str or type(value) == unicode:
                        value = value.encode('utf-8', 'replace')
                d[key]=value
        return d



form = cgi.FieldStorage()

MythLog._setlevel('none')
#mythDB = MythDB()
mythDB = MythDB(args=(('DBHostName',myDBHostName),('DBName',myDBName),('DBUserName',myDBUserName),('DBPassword',myDBPassword)))
mythXML = MythXML(db=mythDB)
mythVideo = MythVideo(db=mythDB)

try :
	myBackend = form['backend'].value
	mythBE = MythBE(myBackend, db=mythDB)
except KeyError:
	mythBE = MythBE(db=mythDB)


#MySQL connection
sqlDB = MySQLdb.connect(host = myDBHostName,user = myDBUserName,passwd = myDBPassword,db = myDBName)

#So we can create JSON values from SQL
def FetchOneAssoc(cursor) :
    data = cursor.fetchone()
    if data == None :
        return None
    desc = cursor.description

    dict = {}

    for (name, value) in zip(desc, data) :
        dict[name[0]] = value

    return dict


createJson = 0
header = 'json'


ext2conttype = {"jpg": "image/jpeg",
				"jpeg": "image/jpeg",
				"png": "image/png",
				"gif": "image/gif", 
				"mp3": "audio/mpeg",
				"avi": "video/avi",
				"mpg": "video/mpeg",
				"mp4": "video/mpeg"}		


def content_type(filename):
	return ext2conttype[filename[filename.rfind(".")+1:].lower()]



try :
	op = form['op'].value
except KeyError:
	op = 'Missing'
	

	
if op == 'getScriptVersion':
	#return script version
	result = version
	
elif op == 'getSQL':
	createJson = 1
	
	table = form['table'].value
	
	cursor = sqlDB.cursor(cursorclass=MySQLdb.cursors.DictCursor)
	cursor.execute("SELECT * FROM "+table)
	alldata = cursor.fetchall ()
	
	cursor.close()
	sqlDB.close()
	
elif op == 'executeSQL':
	
	query64 = form['query64'].value
	
	cursor = sqlDB.cursor(cursorclass=MySQLdb.cursors.DictCursor)
	cursor.execute(base64.b64decode(query64))
	
	cursor.close()
	sqlDB.close()
	
	result = "Finished running SQL: "+base64.b64decode(query64)
	
elif op == 'executeSQLwithResponse':
	createJson = 1
	
	query64 = form['query64'].value
	
	cursor = sqlDB.cursor(cursorclass=MySQLdb.cursors.DictCursor)
	cursor.execute(base64.b64decode(query64))
	alldata = cursor.fetchall ()
	
	cursor.close()
	sqlDB.close()
	
elif op == 'getRecord':
	createJson = 2
	
	recordId = form['recordId'].value
	
	cursor = sqlDB.cursor(cursorclass=MySQLdb.cursors.DictCursor)
	cursor.execute("SELECT * FROM record WHERE recordid = "+recordId)
	alldata = cursor.fetchone ()
	
	cursor.close()
	sqlDB.close()
	
elif op == 'reschedule':

	try :
		recordId_in = form['recordId'].value
	except KeyError:
		recordId_in = '-1'
	
	mythBE.reschedule(recordid=recordId_in)
	
	result = "started scheduler"
	
elif op == 'backendCommand':

	command64 = form['command64'].value
	
	result = mythBE.backendCommand(base64.b64decode(command64))
	
elif op == 'getFile':
	header = 'file'
	filename = form['filename'].value
	dir = form['dir'].value

elif op == 'downloadFile':
	header = 'download'
	filename = form['filename'].value
	dir = form['dir'].value
	
elif op == 'getPremadeImage':
	header = 'file'
	
	chanid_in = form['chanid'].value
	starttime_a = form['starttime'].value
	
	starttime_b = string.replace(starttime_a, '-', '')
	starttime_c = string.replace(starttime_b, ':', '')
	starttime_in = string.replace(starttime_c, ' ', '')
	
	
	filename = mythBE.getCheckfile(mythBE.getRecording(int(chanid_in), int(starttime_in)))+'.png'
	dir = '/'
	
elif op == 'getUpcoming':
	#asking for upcoming recordings - will record
	createJson = 1
	alldata = mythBE.getUpcomingRecordings()
		
elif op == 'getConflicting':
	#asking for upcoming recordings - conflicting
	#only added in 0.24
	
	createJson = 1
	alldata = mythBE.getConflictedRecordings()
	
elif op == 'getPending' :
	#asking for pending - matches recording rule
	createJson = 1
	alldata = mythBE.getPendingRecordings()
	
elif op == 'getScheduled' :
	#asking for scheduled - scheduled to recored
	createJson = 1
	alldata = mythBE.getScheduledRecordings()
	
elif op == 'getRecorded' :
	#asking for recorded programs
	createJson = 1
	alldata = mythBE.getRecordings()

elif op == 'getExpiring' :
	createJson = 1
	alldata = mythBE.getExpiring()

elif op == 'getStorageGroup' :
	#get all storage groups
	createJson = 1
	alldata = mythDB.getStorageGroup()

elif op == 'getChannels' :
	createJson = 1
	alldata = mythDB.getChannels()

elif op == 'getRecorderList' :
	result = mythBE.getRecorderList()

elif op == 'getFreeRecorderList' :
	result = mythBE.getFreeRecorderList()

elif op == 'getCurrentRecording' :
	createJson = 2
	recorder = form['recorder'].value

	alldata = mythBE.getCurrentRecording(recorder)

elif op == 'isRecording' :
	recorder = form['recorder'].value

	result = mythBE.isRecording(recorder)

elif op == 'isActiveBackend' :
	host = form['host'].value

	result = mythBE.isActiveBackend(host)

elif op == 'getFreeSpace' :
	createJson = 1

	alldata = mythBE.getFreeSpace(1)

elif op == 'getLoad' :
	#createJson = 2

	result = mythBE.getLoad()

elif op == 'getUptime' :

	result = mythBE.getUptime()

elif op == 'getVideos':
	#asking for all videos
	createJson = 1
	
	alldata = mythVideo.searchVideos(insertedafter='1900-01-01 00:00:00')
	#alldata = mythVideo.searchVideos(insertedafter='2010-08-30 00:08:09')

elif op == 'getVideoData':
	#asking for a single video data
	createJson = 2
	
	filename = form['filename'].value
	
	alldata = mythVideo.getVideo(file=filename)
	#alldata = mythVideo.searchVideos(insertedafter='2010-08-30 00:08:09')

elif op == 'getVideo':
	#asking for a single video file
	header = 'filePointer'
	
	filename = form['filename'].value
	
	vid = mythVideo.getVideo(file=filename)
	
	myFile = vid.open()
	#alldata = mythVideo.searchVideos(insertedafter='2010-08-30 00:08:09')

elif op == 'getVideoScreenshot':
	#asking for a single video screenshot
	header = 'filePointer'
	
	filename = form['filename'].value
	
	vid = mythVideo.getVideo(file=filename)
	
	myFile = vid.openScreenshot()
	#alldata = mythVideo.searchVideos(insertedafter='2010-08-30 00:08:09')

elif op == 'getVideoFanart':
	#asking for a single video screenshot
	header = 'filePointer'
	
	filename = form['filename'].value
	
	vid = mythVideo.getVideo(file=filename)
	
	myFile = vid.openFanart()
	#alldata = mythVideo.searchVideos(insertedafter='2010-08-30 00:08:09')

elif op == 'getVideoCoverart':
	#asking for a single video coverart
	header = 'filePointer'
	
	filename = form['filename'].value
	
	vid = mythVideo.getVideo(file=filename)
	
	myFile = vid.openCoverart()
	#alldata = mythVideo.searchVideos(insertedafter='2010-08-30 00:08:09')

elif op == 'getVideoBanner':
	#asking for a single video banner
	header = 'filePointer'
	
	filename = form['filename'].value
	
	vid = mythVideo.getVideo(file=filename)
	
	myFile = vid.openBanner()
	#alldata = mythVideo.searchVideos(insertedafter='2010-08-30 00:08:09')
	
elif op == 'scanVideos':
	#scns video storage groups on all backends
	
	videoData = mythVideo.scanStorageGroups()
	
	result = "scanned storage groups"
	
elif op == 'getMusic':
	createJson = 1
	
	
	cursor = sqlDB.cursor(cursorclass=MySQLdb.cursors.DictCursor)
	cursor.execute("SELECT music_songs.song_id, music_songs.name, music_songs.filename, music_songs.year, music_songs.track, music_artists.artist_name, music_albums.album_name, music_albums.year AS album_year, music_albums.compilation FROM music_songs, music_artists, music_albums WHERE music_songs.artist_id = music_artists.artist_id AND music_songs.album_id = music_albums.album_id")
	alldata = cursor.fetchall ()
	
	cursor.close()
	sqlDB.close()

elif op == 'getRecording' :
	#asking for a singled recorded program
	createJson = 2
	
	chanid_in = form['chanid'].value
	starttime_a = form['starttime'].value
	
	starttime_b = string.replace(starttime_a, '-', '')
	starttime_c = string.replace(starttime_b, ':', '')
	starttime_in = string.replace(starttime_c, ' ', '')
	
	alldata = mythBE.getRecording(int(chanid_in), int(starttime_in))

elif op == 'getCheckfile' :
	# requires a single recording input method

	chanid_in = form['chanid'].value
	starttime_a = form['starttime'].value
	
	starttime_b = string.replace(starttime_a, '-', '')
	starttime_c = string.replace(starttime_b, ':', '')
	starttime_in = string.replace(starttime_c, ' ', '')
	
	result = mythBE.getCheckfile(mythBE.getRecording(int(chanid_in), int(starttime_in)))
	
elif op == 'getSetting' :
	#asking for a XML setting
	try :
		result = mythXML.getSetting(form['setting'].value, form['host'].value)
	except KeyError:
		result = mythXML.getSetting(form['setting'].value)
	
	#result = "getSettings currently not working"
	
elif op == 'getFrontends' :
	#asking for all frontends
	#initialresult = mythDB.getFrontends()
	
	result = "getFrontends not currently working"
	#result = initialresult[len(initialresult) - 1]

elif op == 'getServDesc' :

	result = mythXML.getServDesc()
	
elif op == 'getHosts' :
	#asking for all hosts - error-proof way to get all hosts
	initialresult = mythXML.getHosts()
	#count = 0
	
	result = string.replace(str(initialresult), "'", '"')
	result = string.replace(result, '[', '[ { "hostname": ')
	result = string.replace(result, ',', ', "port": "6546" }, { "hostname": ')
	result = string.replace(result, ']', ', "port": "6546" } ]')

elif op == 'getKeys' :
	#not sure what this is - just seems to list all keys but cannot execute

	result = mythXML.getKeys()


#try getSetting from XML

elif op == 'getProgramGuide' :
	#createJson = 1

	endtime_a = form['endtime'].value
	starttime_a = form['starttime'].value
	
	endtime_b = string.replace(endtime_a, '-', '')
	endtime_c = string.replace(endtime_b, ':', '')
	endtime_in = string.replace(endtime_c, ' ', '')
	
	starttime_b = string.replace(starttime_a, '-', '')
	starttime_c = string.replace(starttime_b, ':', '')
	starttime_in = string.replace(starttime_c, ' ', '')

	alldata = mythXML.getProgramGuide(int(starttime_in), int(endtime_in))

	#result = "getProgramGuide currently does not work"
	
elif op == 'remote':
	#remote to frontend
	try :
		type = form['type'].value
	except KeyError:
		type = 'unknown'
	
	host = form['host'].value
	command = string.replace(form['cmd'].value,"+", " ")
	
	frontend = mythDB.getFrontend(host)
	
	if type == 'key' :	
		frontend.sendKey(command)
		result = '{ "value" : "success" }'
	elif type == 'jump' :
		frontend.sendJump(command)
		result = '{ "value" : "success" }'
	elif type == 'query' :
		result = frontend.sendQuery(command)
	elif type == 'play' :
		frontend.sendPlay(command)
		result = '{ "value" : "success" }'
	else :
		result = '{ "value" : "ERROR - need to supply type or remote call" }'
		
else :
	#op parameter was not given
	result = "unknown"





	
if createJson == 1:
        l=[repackDicts(data) for data in alldata]
        result = dumps(l)
	
elif createJson == 2:
        d=repackDicts(alldata)
        result = dumps(d)
		
if createJson == 10:
	count = 0
	result = '[ '
	while (count < len(alldata)) :
		result += '{ "'
		singledata = alldata[count]
		for key,value in singledata.items():
			#try:
				#print value
			result += str(key)
			result += '": "'
			try :
				result += string.replace(str(value), '"', '')
			except UnicodeEncodeError :
				try: 
					result += string.replace(str(value.encode('ascii', 'replace')), '"', '')
				except:
					result += 'ERROR - unicode encooding error'
			result += '", "'
		result = result[:-3]
		result += ' }, \n'
		count = count+1

	result = result[:-3]
	result += ' ]'

elif createJson == 20:
	count = 0
	singledata = alldata
	result = '[ { "'
	for key,value in singledata.items():
		result += str(key)
		result += '": "'
		try :
			result += string.replace(str(value), '"', '')
		except UnicodeEncodeError :
			try: 
				result += string.replace(str(value.encode('ascii', 'replace')), '"', '')
			except:
				result += 'ERROR - unicode encooding error'
		result += '", "'
	result = result[:-3]
	result += " } ]"
#


if header == 'json' :
	print "Content-type: application/json"
	print ""
	print result
elif header == 'file' :
	print "Content-type: %s " % (content_type(dir+filename))
	print "Content-Length: %d " % int(os.path.getsize(dir+filename))
	print ""
	print file(dir+filename, "r").read()
elif header == 'filePointer' :
	print "Content-type: %s " % (content_type(myFile))
	#print "Content-Length: %d " % int(os.path.getsize(file))
	print ""
	print myFile
elif header == 'download' :
	print "Content-type: application/octet-stream; name=%s " % filename
	print "Content-Disposition: attachment; filename=%s " % filename
	print "Content-Length: %d " % int(os.path.getsize(dir+filename))
	print ""
	#print str(os.path.getsize(dir+filename))
	print file(dir+filename, "r").read()
	