all:
	make full

full:
	rm -f ../AdditionalQuests.zip
	zip ../AdditionalQuests.zip -r ./bin ./Config/ SubModule.xml README.md
