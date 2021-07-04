all:
	make full

full:
	rm -f ../AdditionalQuests.zip
	# Create Temp folder for zipping
	mkdir AdditionalQuests
	cp -r ./bin AdditionalQuests
	cp SubModule.xml AdditionalQuests
	cp README.md AdditionalQuests
	cp LICENSE AdditionalQuests
	zip ../AdditionalQuests.zip -r AdditionalQuests
	rm -r AdditionalQuests