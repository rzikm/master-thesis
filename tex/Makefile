export TEXINPUTS=../tex//:

IMGDIR=img

IMAGES_SRC=$(wildcard $(IMGDIR)/*.svg)
IMAGES_OUT=$(patsubst %.svg, %.pdf_tex, $(IMAGES_SRC))
IMAGES_PDF=$(patsubst %.svg, %.pdf, $(IMAGES_SRC))

all: thesis.pdf abstract.pdf

# LaTeX must be run multiple times to get references right
thesis.pdf: thesis.tex $(wildcard *.tex) bibliography.bib thesis.xmpdata $(IMAGES_OUT)
	pdflatex $<
	bibtex thesis
	pdflatex $<
	pdflatex $<

abstract.pdf: abstract.tex abstract.xmpdata
	pdflatex $<

$(IMGDIR)/%.pdf_tex: $(IMGDIR)/%.svg
	inkscape --export-filename $(patsubst %.svg, %.pdf, $<) --export-latex $<

images: $(IMAGES_OUT)

clean:
	rm -f *.log *.dvi *.aux *.toc *.lof *.lot *.out *.bbl *.blg *.xmpi
	rm -f thesis.pdf abstract.pdf
	rm -f $(IMAGES_PDF) $(IMAGES_OUT)
